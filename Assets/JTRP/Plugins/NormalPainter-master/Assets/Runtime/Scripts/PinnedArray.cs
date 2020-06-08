using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UTJ.NormalPainter
{

    public class PinnedObject<T> : IDisposable
    {
        T m_data;
        GCHandle m_gch;

        public PinnedObject(T data)
        {
            m_data = data;
            m_gch = GCHandle.Alloc(m_data, GCHandleType.Pinned);
        }

        public T Object { get { return m_data; } }
        public IntPtr Pointer { get { return m_gch.AddrOfPinnedObject(); } }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_gch.IsAllocated)
                    m_gch.Free();
            }
        }

        public static implicit operator IntPtr(PinnedObject<T> v) { return v.Pointer; }
    }


    public class PinnedArray<T> : IDisposable, IEnumerable<T> where T : struct
    {
        T[] m_data;
        GCHandle m_gch;

        public PinnedArray(int size)
        {
            m_data = new T[size];
            m_gch = GCHandle.Alloc(m_data, GCHandleType.Pinned);
        }
        public PinnedArray(T[] data, bool clone = false)
        {
            m_data = clone ? (T[])data.Clone() : data;
            m_gch = GCHandle.Alloc(m_data, GCHandleType.Pinned);
        }

        public int Length { get { return m_data.Length; } }
        public T this[int i]
        {
            get { return m_data[i]; }
            set { m_data[i] = value; }
        }
        public T[] Array { get { return m_data; } }
        public IntPtr Pointer { get { return m_data.Length == 0 ? IntPtr.Zero : m_gch.AddrOfPinnedObject(); } }

        public PinnedArray<T> Clone() { return new PinnedArray<T>((T[])m_data.Clone()); }
        public bool Assign(T[] source)
        {
            if (source != null && m_data.Length == source.Length)
            {
                System.Array.Copy(source, m_data, m_data.Length);
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_gch.IsAllocated)
                    m_gch.Free();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)m_data.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator IntPtr(PinnedArray<T> v) { return v == null ? IntPtr.Zero : v.Pointer; }
    }


    public class PinnedArray2D<T> : IDisposable, IEnumerable<T>
    {
        T[,] m_data;
        GCHandle m_gch;

        public PinnedArray2D(int x, int y)
        {
            m_data = new T[x, y];
            m_gch = GCHandle.Alloc(m_data, GCHandleType.Pinned);
        }
        public PinnedArray2D(T[,] data, bool clone = false)
        {
            m_data = clone ? (T[,])data.Clone() : data;
            m_gch = GCHandle.Alloc(m_data, GCHandleType.Pinned);
        }

        public int Length { get { return m_data.Length; } }
        public T this[int x, int y]
        {
            get { return m_data[x, y]; }
            set { m_data[x, y] = value; }
        }
        public T[,] Array { get { return m_data; } }
        public IntPtr Pointer { get { return m_data.Length == 0 ? IntPtr.Zero : m_gch.AddrOfPinnedObject(); } }

        public PinnedArray2D<T> Clone() { return new PinnedArray2D<T>((T[,])m_data.Clone()); }
        public bool Assign(T[,] source)
        {
            if (source != null && m_data.Length == source.Length)
            {
                System.Array.Copy(source, m_data, m_data.Length);
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_gch.IsAllocated)
                    m_gch.Free();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)m_data.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator IntPtr(PinnedArray2D<T> v) { return v == null ? IntPtr.Zero : v.Pointer; }
    }


    #region dirty
    public static class PinnedListImpl
    {
        class ListData
        {
            public object items;
            public int size;
        }
        [StructLayout(LayoutKind.Explicit)]
        struct Caster
        {
            [FieldOffset(0)] public object list;
            [FieldOffset(0)] public ListData data;
        }

        public static T[] GetInternalArray<T>(List<T> list) where T : struct
        {
            var caster = new Caster();
            caster.list = list;
            return (T[])caster.data.items;
        }
        public static List<T> CreateIntrusiveList<T>(T[] data) where T : struct
        {
            var ret = new List<T>();
            var caster = new Caster();
            caster.list = ret;
            caster.data.items = data;
            caster.data.size = data.Length;
            return ret;
        }
        public static void SetCount<T>(List<T> list, int count) where T : struct
        {
            var caster = new Caster();
            caster.list = list;
            caster.data.size = count;
        }
    }
    #endregion


    // Pinned"List" but assume size is fixed (== functionality is same as PinnedArray).
    // this class is intended to pass to Mesh.GetNormals(), Mesh.SetNormals(), and C++ functions.
    public class PinnedList<T> : IDisposable, IEnumerable<T> where T : struct
    {
        List<T> m_list;
        T[] m_data;
        GCHandle m_gch;

        public PinnedList(int size = 0)
        {
            m_data = new T[size];
            m_list = PinnedListImpl.CreateIntrusiveList(m_data);
            m_gch = GCHandle.Alloc(m_data, GCHandleType.Pinned);
        }
        public PinnedList(T[] data, bool clone = false)
        {
            m_data = clone ? (T[])data.Clone() : data;
            m_list = PinnedListImpl.CreateIntrusiveList(m_data);
            m_gch = GCHandle.Alloc(m_data, GCHandleType.Pinned);
        }
        public PinnedList(List<T> data, bool clone = false)
        {
            m_list = clone ? new List<T>(data) : data;
            m_data = PinnedListImpl.GetInternalArray(m_list);
            m_gch = GCHandle.Alloc(m_data, GCHandleType.Pinned);
        }


        public int Capacity { get { return m_data.Length; } }
        public int Count { get { return m_list.Count; } }
        public T this[int i]
        {
            get { return m_data[i]; }
            set { m_data[i] = value; }
        }
        public T[] Array { get { return m_data; } }
        public List<T> List { get { return m_list; } }
        public IntPtr Pointer { get { return Count == 0 ? IntPtr.Zero : m_gch.AddrOfPinnedObject(); } }

        public void LockList(Action<List<T>> body)
        {
            if (m_gch.IsAllocated)
                m_gch.Free();
            body(m_list);
            m_data = PinnedListImpl.GetInternalArray(m_list);
            m_gch = GCHandle.Alloc(m_data, GCHandleType.Pinned);
        }

        public void Resize(int size)
        {
            if (size > m_data.Length)
            {
                LockList(l =>
                {
                    l.Capacity = size;
                });
            }
            PinnedListImpl.SetCount(m_list, size);
        }

        public void ResizeDiscard(int size)
        {
            if (size > m_data.Length)
            {
                if (m_gch.IsAllocated)
                    m_gch.Free();
                m_data = new T[size];
                m_list = PinnedListImpl.CreateIntrusiveList(m_data);
                m_gch = GCHandle.Alloc(m_data, GCHandleType.Pinned);
            }
            else
            {
                PinnedListImpl.SetCount(m_list, size);
            }
        }

        public void Clear()
        {
            if (m_data.Length > 0)
                PinnedListImpl.SetCount(m_list, 0);
        }

        public PinnedList<T> Clone()
        {
            return new PinnedList<T>(m_list, true);
        }

        public void Assign(T[] source)
        {
            ResizeDiscard(source.Length);
            System.Array.Copy(source, m_data, source.Length);
        }
        public void Assign(List<T> sourceList)
        {
            var sourceData = PinnedListImpl.GetInternalArray(sourceList);
            var count = sourceList.Count;
            ResizeDiscard(count);
            System.Array.Copy(sourceData, m_data, count);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_gch.IsAllocated)
                    m_gch.Free();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)m_data.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator IntPtr(PinnedList<T> v) { return v == null ? IntPtr.Zero : v.Pointer; }
    }

}
