using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Unity.Barracuda;
using UnityEngine.Experimental.Rendering;

// https://blog.unity.com/technology/real-time-style-transfer-in-unity-using-deep-neural-networks
// note: Rendering will be delayed by one frame due to pipeline differences
namespace JTRP.PostProcess
{
	public enum ModelType
	{
		Reference        = 0, // paper中未经优化的模型
		RefBut32Channels = 1  // 优化后的模型
	}

	[Serializable]
	public class WorkerFactoryTypeParameter : VolumeParameter<WorkerFactory.Type>
	{
		public WorkerFactoryTypeParameter(WorkerFactory.Type value, bool overrideState = false)
			: base(value, overrideState) { }
	}

	[Serializable]
	public class StyleTexturesParameter : VolumeParameter<List<Texture2D>>
	{
		public StyleTexturesParameter(List<Texture2D> value, bool overrideState = false)
			: base(value, overrideState) { }
	}

	[Serializable, VolumeComponentMenu("JTRP/StyleTransfer")]
	public class StyleTransfer : CustomPostProcessVolumeComponent, IPostProcessComponent
	{
		public Vector2Parameter                   inputResolution                = new Vector2Parameter(new Vector2Int(960, 540));
		public BoolParameter                      forceBilinearUpsample2DInModel = new BoolParameter(true);
		public WorkerFactoryTypeParameter         workerType                     = new WorkerFactoryTypeParameter(WorkerFactory.Type.Auto);
		public BoolParameter                      debugModelLoading              = new BoolParameter(false);
		public CloudLayerEnumParameter<ModelType> modelType                      = new CloudLayerEnumParameter<ModelType>(ModelType.RefBut32Channels);
		public ClampedFloatParameter              pregamma                       = new ClampedFloatParameter(1.0f, 0.001f, 5.0f);
		public ClampedFloatParameter              postgamma                      = new ClampedFloatParameter(2.2f, 0.001f, 5.0f);
		public BoolParameter                      showStylePreview               = new BoolParameter(true);
		public IntParameter                       styleTextureIndex              = new IntParameter(0);
		public StyleTexturesParameter             styleTextures                  = new StyleTexturesParameter(null);

		// 由于API的冲突, 处理的是上一帧的图像, BeforePostProcess时会覆盖所有后处理, 所以必须为AfterPostProcess
		public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

		// The compiled model used for performing inference
		private Model _model;

		// The interface used to execute the neural network
		private IWorker _worker;

		// The Material used to gamma correction
		private Material _pregammaMat, _postgammaMat;

		private          RTHandle      _rtHandle, _rtHandlePregamma, _rtHandlePostgamma, _rtHandleStyle;
		private          NNModel[]     _nnModels;
		private          List<float[]> _predictionAlphasBetasData;
		private          Texture2D     _lastStyle;
		private          Tensor        _input;
		private          Tensor        _pred;
		private readonly Vector4       _postNetworkColorBias = new Vector4(0.4850196f, 0.4579569f, 0.4076039f, 0.0f);
		private          List<string>  _layerNameToPatch;

		public bool IsActive() =>
			styleTextures.value != null
		 && styleTextures.value.Count > 0;

		private Texture2D _styleTexture =>
			IsActive() ? styleTextures.value[styleTextureIndex.value] : null;

		public override void Setup()
		{
			Debug.Log("Setup");

			// Load assets from Resources folder
			_nnModels = new NNModel[]
			{
				Resources.Load<NNModel>("adele_2"),
				Resources.Load<NNModel>("model_32channels")
			};

			Debug.Assert(Enum.GetNames(typeof(ModelType)).Length == _nnModels.Length);
			Debug.Assert(_nnModels.All(m => m != null));

			ComputeInfo.channelsOrder = ComputeInfo.ChannelsOrder.NCHW;

			// Compile the model asset into an object oriented representation
			_model = ModelLoader.Load(_nnModels[(int)modelType.value], debugModelLoading.value);

			float scale = 1; //inputResolution.value.y / (float)Screen.height;

			_rtHandle = RTHandles.Alloc(
										colorFormat: GraphicsFormat.R16G16B16A16_UNorm,
										scaleFactor: Vector2.one * scale,
										wrapMode: TextureWrapMode.Clamp,
										enableRandomWrite: true
									   );
			_rtHandlePregamma = RTHandles.Alloc(
												colorFormat: GraphicsFormat.R16G16B16A16_UNorm,
												scaleFactor: Vector2.one * scale,
												wrapMode: TextureWrapMode.Clamp,
												enableRandomWrite: true
											   );
			_rtHandlePostgamma = RTHandles.Alloc(
												 colorFormat: GraphicsFormat.R16G16B16A16_UNorm,
												 scaleFactor: Vector2.one * scale,
												 wrapMode: TextureWrapMode.Clamp,
												 enableRandomWrite: true
												);
			_rtHandleStyle = RTHandles.Alloc(
												 256, 256,
												 colorFormat: GraphicsFormat.R16G16B16A16_SFloat,
												 wrapMode: TextureWrapMode.Clamp,
												 enableRandomWrite: true
												);

			_pregammaMat = new Material(Shader.Find("Barracuda/Activation"));
			_pregammaMat.EnableKeyword("Pow");
			_pregammaMat.EnableKeyword("BATCHTILLING_ON");
			_postgammaMat = new Material(_pregammaMat);
			_postgammaMat.CopyPropertiesFromMaterial(_pregammaMat);

			//Prepare style transfer prediction and runtime worker at load time (to avoid memory allocation at runtime)
			PrepareStylePrediction();
			CreateBarracudaWorker();
		}

		public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
		{
			Debug.Log("Render");
			if (!IsActive()) return;
			if (_styleTexture == null || _worker == null || _pregammaMat == null) Setup();
			if (_lastStyle != _styleTexture)
			{
				_lastStyle = _styleTexture;
				PrepareStylePrediction();
				PatchRuntimeWorkerWithStylePrediction();
				cmd.Blit(_styleTexture, _rtHandleStyle);
			}

			// Source cannot be used before blit, the reason is unknown
			cmd.Blit(source, _rtHandle, 0, 0);

			// 模型在srgb空间中训练, 输入的buffer格式应为UNorm, 内容应为srgb, 可以通过renderdoc查看
			_pregammaMat.SetVector("XdeclShape", new Vector4(1, _rtHandle.rt.height, _rtHandle.rt.width, 3));
			_pregammaMat.SetVector("OdeclShape", new Vector4(1, _rtHandle.rt.height, _rtHandle.rt.width, 3));
			_pregammaMat.SetTexture("Xdata", _rtHandle);
			_pregammaMat.SetTexture("Odata", _rtHandlePregamma);
			_pregammaMat.SetFloat("_Alpha", pregamma.value);
			cmd.Blit(null, _rtHandlePregamma, _pregammaMat);

			_input = new Tensor(_rtHandlePregamma, 3);
			Dictionary<string, Tensor> temp = new Dictionary<string, Tensor>();
			temp.Add("frame", _input);
			_worker.Execute(temp);
			_input.Dispose();
			_pred = _worker.PeekOutput();
			_pred.ToRenderTexture(_rtHandlePostgamma, 0, 0, Vector4.one, _postNetworkColorBias);

			// 模型输出理论上也应为srgb, 但由于API混乱不清, 根据截帧结果得知此处需要linear to srgb转换
			_postgammaMat.CopyPropertiesFromMaterial(_pregammaMat);
			_postgammaMat.SetTexture("Xdata", _rtHandlePostgamma);
			_postgammaMat.SetTexture("Odata", destination);
			_postgammaMat.SetFloat("_Alpha", postgamma.value);

			cmd.Blit(null, destination, _postgammaMat);
			if (showStylePreview.value)
				cmd.CopyTexture(_rtHandleStyle, 0, 0, 0, 0, _rtHandleStyle.rt.width, _rtHandleStyle.rt.height, destination, 0, 0, 0, 0);
			_pred.Dispose();
		}

		public override void Cleanup()
		{
			Debug.Log("Cleanup");
			_lastStyle = null;
			if (_worker != null)
				_worker.Dispose();
			if (_rtHandle != null)
				_rtHandle.Release();
			if (_rtHandlePregamma != null)
				_rtHandlePregamma.Release();
			if (_rtHandlePostgamma != null)
				_rtHandlePostgamma.Release();
			if (_rtHandleStyle != null)
				_rtHandleStyle.Release();
			if (_input != null)
				_input.Dispose();
			if (_pred != null)
				_pred.Dispose();
		}

		// https://github.com/JasonMa0012/barracuda-style-transfer/blob/fb61d2d8172e3150f6ebbfb78443d0fe9def66db/Assets/BarracudaStyleTransfer/BarracudaStyleTransfer.cs#L575
		private void PrepareStylePrediction()
		{
			if (_styleTexture == null) return;

			Model tempModel = ModelLoader.Load(_nnModels[(int)modelType.value], debugModelLoading.value); //_model.ShallowCopy();
			List<Layer> predictionAlphasBetas = new List<Layer>();
			List<Layer> layerList = new List<Layer>(tempModel.layers);

			// Remove Divide by 255, Unity textures are in [0, 1] already
			int firstDivide = FindLayerIndexByName(layerList, "Style_Prediction_Network/normalized_image");
			if (firstDivide < 0)
				Debug.Log(0);
			layerList[firstDivide + 1].inputs[0] = layerList[firstDivide].inputs[0];
			layerList.RemoveAt(firstDivide);

			// Pre-process network to get it to run and extract Style alpha/beta tensors
			Layer lastConv = null;
			for (int i = 0; i < layerList.Count; i++)
			{
				Layer layer = layerList[i];

				// Remove Mirror padding layers (not supported, TODO)
				if (layer.name.Contains("reflect_padding"))
				{
					layerList[i + 1].inputs = layer.inputs;
					layerList[i + 1].pad = layer.pad.ToArray();
					layerList.RemoveAt(i);
					i--;
					continue;
				}
				// Placeholder instance norm bias + scale tensors
				if (layer.type == Layer.Type.Conv2D || layer.type == Layer.Type.Conv2DTrans)
				{
					lastConv = layer;
				}
				else if (layer.type == Layer.Type.Normalization)
				{
					int channels = lastConv.datasets[1].shape.channels;
					layer.datasets = new Layer.DataSet[2];

					layer.datasets[0].shape = new TensorShape(1, 1, 1, channels);
					layer.datasets[0].offset = 0;
					layer.datasets[0].length = channels;

					layer.datasets[1].shape = new TensorShape(1, 1, 1, channels);
					layer.datasets[1].offset = channels;
					layer.datasets[1].length = channels;

					float[] data = new float[channels * 2];
					for (int j = 0; j < data.Length / 2; j++)
						data[j] = 1.0f;
					for (int j = data.Length / 2; j < data.Length; j++)
						data[j] = 0.0f;
					layer.weights = new BarracudaArrayFromManagedArray(data);
				}

				if (layer.type != Layer.Type.StridedSlice && layer.name.Contains("StyleNetwork/"))
				{
					layerList.RemoveAt(i);
					i--;
				}

				if (layer.type == Layer.Type.StridedSlice)
				{
					predictionAlphasBetas.Add(layer);
				}
			}
			tempModel.layers = layerList;
			// Run Style_Prediction_Network on given style
			var styleInput = new Tensor(_styleTexture);
			Dictionary<string, Tensor> temp = new Dictionary<string, Tensor>();
			temp.Add("frame", styleInput);
			temp.Add("style", styleInput);
			IWorker tempWorker = WorkerFactory.CreateWorker(WorkerFactory.ValidateType(workerType.value), tempModel, debugModelLoading.value);
			tempWorker.Execute(temp);

			// Store alpha/beta tensors from Style_Prediction_Network to feed into the run-time network
			_predictionAlphasBetasData = new List<float[]>();
			foreach (var layer in predictionAlphasBetas)
			{
				_predictionAlphasBetasData.Add(tempWorker.PeekOutput(layer.name).ToReadOnlyArray());
			}

			
			tempWorker.Dispose();
			styleInput.Dispose();

			Debug.Log("Style Prediction Model: \n" + tempModel.ToString());
		}

		private void CreateBarracudaWorker()
		{
			if (_styleTexture == null || _predictionAlphasBetasData == null) return;
			int savedAlphaBetasIndex = 0;
			_layerNameToPatch = new List<string>();
			List<Layer> layerList = new List<Layer>(_model.layers);

			// Pre-process Network for run-time use
			Layer lastConv = null;
			for (int i = 0; i < layerList.Count; i++)
			{
				Layer layer = layerList[i];

				// Remove Style_Prediction_Network: constant with style, executed once in Setup()
				if (layer.name.Contains("Style_Prediction_Network/"))
				{
					layerList.RemoveAt(i);
					i--;
					continue;
				}

				// Fix Upsample2D size parameters
				if (layer.type == Layer.Type.Upsample2D)
				{
					layer.pool = new[] { 2, 2 };
					//ref model is supposed to be nearest sampling but bilinear scale better when network is applied at lower resoltions
					bool useBilinearUpsampling = forceBilinearUpsample2DInModel.value || (modelType.value != ModelType.Reference);
					layer.axis = useBilinearUpsampling ? 1 : -1;
				}

				// Remove Mirror padding layers (not supported, TODO)
				if (layer.name.Contains("reflect_padding"))
				{
					layerList[i + 1].inputs = layer.inputs;
					layerList[i + 1].pad = layer.pad.ToArray();
					layerList.RemoveAt(i);
					i--;
				}
				else if (layer.type == Layer.Type.Conv2D || layer.type == Layer.Type.Conv2DTrans)
				{
					lastConv = layer;
				}
				else if (layer.type == Layer.Type.Normalization)
				{
					// Manually set alpha/betas from Style_Prediction_Network as scale/bias tensors for InstanceNormalization
					if (layerList[i - 1].type == Layer.Type.StridedSlice)
					{
						int channels = _predictionAlphasBetasData[savedAlphaBetasIndex].Length;
						layer.datasets = new Layer.DataSet[2];

						layer.datasets[0].shape = new TensorShape(1, 1, 1, channels);
						layer.datasets[0].offset = 0;
						layer.datasets[0].length = channels;

						layer.datasets[1].shape = new TensorShape(1, 1, 1, channels);
						layer.datasets[1].offset = channels;
						layer.datasets[1].length = channels;

						_layerNameToPatch.Add(layer.name);

						float[] data = new float[channels * 2];
						for (int j = 0; j < data.Length / 2; j++)
							data[j] = _predictionAlphasBetasData[savedAlphaBetasIndex][j];
						for (int j = data.Length / 2; j < data.Length; j++)
							data[j] = _predictionAlphasBetasData[savedAlphaBetasIndex + 1][j - data.Length / 2];

						layer.weights = new BarracudaArrayFromManagedArray(data);

						savedAlphaBetasIndex += 2;
					}
					// Else initialize scale/bias tensors of InstanceNormalization to default 1/0
					else
					{
						int channels = lastConv.datasets[1].shape.channels;
						layer.datasets = new Layer.DataSet[2];

						layer.datasets[0].shape = new TensorShape(1, 1, 1, channels);
						layer.datasets[0].offset = 0;
						layer.datasets[0].length = channels;

						layer.datasets[1].shape = new TensorShape(1, 1, 1, channels);
						layer.datasets[1].offset = channels;
						layer.datasets[1].length = channels;

						float[] data = new float[channels * 2];
						for (int j = 0; j < data.Length / 2; j++)
							data[j] = 1.0f;
						for (int j = data.Length / 2; j < data.Length; j++)
							data[j] = 0.0f;
						layer.weights = new BarracudaArrayFromManagedArray(data);
					}
				}
			}

			// Remove Slice layers originally used to get alpha/beta tensors into Style_Network
			for (int i = 0; i < layerList.Count; i++)
			{
				Layer layer = layerList[i];
				if (layer.type == Layer.Type.StridedSlice)
				{
					layerList.RemoveAt(i);
					i--;
				}
			}

			// Fold Relu into instance normalisation
			Dictionary<string, string> reluToInstNorm = new Dictionary<string, string>();
			for (int i = 0; i < layerList.Count; i++)
			{
				Layer layer = layerList[i];
				if (layer.type == Layer.Type.Activation && layer.activation == Layer.Activation.Relu)
				{
					if (layerList[i - 1].type == Layer.Type.Normalization)
					{
						layerList[i - 1].activation = layer.activation;
						reluToInstNorm[layer.name] = layerList[i - 1].name;
						layerList.RemoveAt(i);
						i--;
					}
				}
			}
			for (int i = 0; i < layerList.Count; i++)
			{
				Layer layer = layerList[i];
				for (int j = 0; j < layer.inputs.Length; j++)
				{
					if (reluToInstNorm.ContainsKey(layer.inputs[j]))
					{
						layer.inputs[j] = reluToInstNorm[layer.inputs[j]];
					}
				}
			}

			// Feed first convolution directly with input (no need for normalisation from the model)
			string firstConvName = "StyleNetwork/conv1/convolution_conv1/convolution";
			int firstConv = FindLayerIndexByName(layerList, firstConvName);
			layerList[firstConv].inputs = new[] { _model.inputs[1].name };

			if (modelType.value == ModelType.Reference)
			{
				layerList.RemoveAt(FindLayerIndexByName(layerList, "StyleNetwork/normalisation/add"));
				layerList.RemoveAt(FindLayerIndexByName(layerList, "StyleNetwork/normalisation/add/y"));
				layerList.RemoveAt(FindLayerIndexByName(layerList, "StyleNetwork/normalisation/normalized_contentFrames"));
				layerList.RemoveAt(FindLayerIndexByName(layerList, "StyleNetwork/normalisation/normalized_contentFrames/y"));
				layerList.RemoveAt(FindLayerIndexByName(layerList, "StyleNetwork/normalisation/sub"));
				layerList.RemoveAt(FindLayerIndexByName(layerList, "StyleNetwork/normalisation/sub/y"));
			}
			if (modelType.value == ModelType.RefBut32Channels)
			{
				layerList.RemoveAt(FindLayerIndexByName(layerList, "StyleNetwork/normalized_contentFrames"));
				layerList.RemoveAt(FindLayerIndexByName(layerList, "StyleNetwork/normalized_contentFrames/y"));
			}

			// Remove final model post processing, post process happen in tensor to texture instead
			int postAdd = FindLayerIndexByName(layerList, "StyleNetwork/clamp_0_255/add");
			layerList.RemoveRange(postAdd, 5);

			// Correct wrong output layer list
			_model.outputs = new List<string>() { layerList[postAdd - 1].name };

			_model.layers = layerList;
			Model.Input input = _model.inputs[1];
			input.shape[0] = 0;
			input.shape[1] = _rtHandle.rt.height;
			input.shape[2] = _rtHandle.rt.width;
			input.shape[3] = 3;
			_model.inputs = new List<Model.Input> { _model.inputs[1] };
			//Create worker and execute it once at target resolution to prime all memory allocation (however in editor resolution can still change at runtime)
			_worker = WorkerFactory.CreateWorker(WorkerFactory.ValidateType(workerType.value), _model, debugModelLoading.value);
			Dictionary<string, Tensor> temp = new Dictionary<string, Tensor>();
			var inputTensor = new Tensor(input.shape, input.name);
			temp.Add("frame", inputTensor);
			_worker.Execute(temp);
			inputTensor.Dispose();

			Debug.Log("Style Transfer Model: \n" + _model.ToString());
		}

		private void PatchRuntimeWorkerWithStylePrediction()
		{
			if (_layerNameToPatch == null || _predictionAlphasBetasData == null) return;

			int savedAlphaBetasIndex = 0;
			for (int i = 0; i < _layerNameToPatch.Count; ++i)
			{
				var tensors = _worker.PeekConstants(_layerNameToPatch[i]);
				int channels = _predictionAlphasBetasData[savedAlphaBetasIndex].Length;

				// unity官方示例工程这里用了for loop逐个赋值, 这在HDRP中会导致unity crash, 故采用了更简洁的方法
				tensors[0].data.Upload(_predictionAlphasBetasData[savedAlphaBetasIndex], tensors[0].shape);
				tensors[1].data.Upload(_predictionAlphasBetasData[savedAlphaBetasIndex + 1], tensors[1].shape);

				savedAlphaBetasIndex += 2;
			}
		}

		private int FindLayerIndexByName(List<Layer> list, string name)
		{
			return list.FindIndex((layer => layer.name == name));
		}
	}
}