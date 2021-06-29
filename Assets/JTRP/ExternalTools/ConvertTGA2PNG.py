import os
import shutil
from PIL import Image
from multiprocessing import Pool

PATH = "E:\WorkSpace\BarbaraDance\BarbaraDance\Assets"
SRC_EXTENSION = ".tga"
DST_EXTENSION = ".png"
MAX_RESOLUTION = 1024


def check_resolution(im: Image):
    return im.height > MAX_RESOLUTION or im.width > MAX_RESOLUTION


def resize_image(im: Image) -> Image:
    if im.mode == "RGBA":
        alpha = im.getchannel(3)
        im = im.convert("RGB")
        alpha = alpha.resize((MAX_RESOLUTION, MAX_RESOLUTION))
        im = im.resize((MAX_RESOLUTION, MAX_RESOLUTION))
        im.putalpha(alpha)
    else:
        im = im.resize((MAX_RESOLUTION, MAX_RESOLUTION))

    return im


def convert_image(src: str, dst: str):
    if os.path.exists(dst):
        print(dst + "  ======= already exists! =======")
        return
    im = Image.open(src)
    if check_resolution(im):
        im = resize_image(im)
    im.save(dst, DST_EXTENSION[1:], optimizz=True)
    im.close()
    print(dst)


def multi_process(fp: str):
    # save converted image and delete old image
    if fp[-len(SRC_EXTENSION):] == SRC_EXTENSION:
        convert_image(fp, fp[0:-len(SRC_EXTENSION)] + DST_EXTENSION)
        os.remove(fp)
        return

    # rename .meta file
    extension_length = (5 + len(SRC_EXTENSION))
    if fp[-extension_length:] == SRC_EXTENSION + ".meta":
        new_fp = fp[0:-extension_length] + DST_EXTENSION + ".meta"
        if os.path.exists(new_fp):
            print(new_fp + "  ======= already exists! =======")
            return
        shutil.move(fp, new_fp)
        print(new_fp)
        return

    # resize all png
    if fp[-len(DST_EXTENSION):] == DST_EXTENSION:
        im = Image.open(fp)
        if check_resolution(im):
            im = resize_image(im)
            im.save(fp, DST_EXTENSION[1:], optimizz=True)
            print(fp)
        im.close()
        return


if __name__ == '__main__':
    p = Pool()

    for path, dir_list, file_list in os.walk(PATH):
        for file_name in file_list:
            file_path = (os.path.join(path, file_name))
            p.apply_async(multi_process, args=(file_path,))

    print('Waiting for all subprocesses done...')
    p.close()
    p.join()
    print('All subprocesses done.')
