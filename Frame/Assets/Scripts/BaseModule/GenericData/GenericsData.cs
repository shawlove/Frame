using GameFrame.Recycle;

namespace GameFrame.GenericData
{
    public class GenericsData<T> : IGenericData
    {
        public int Index { get; set; }

        private T Data1;

        public GenericsData<T> SetData(T data)
        {
            Data1 = data;
            return this;
        }

        public void Reset()
        {
            Data1 = default;
        }

        public override string ToString()
        {
            return "<color=#708FFE>Data1:</color> " + Data1;
        }

        public void Recycle()
        {
            RecycleList<GenericsData<T>>.Recycle(this);
        }

        public T1 GetData1<T1>()
        {
            if (Data1 is T1 t1)
            {
                return t1;
            }

            return default;
        }

        public T1 GetData2<T1>()
        {
            return default;
        }

        public T1 GetData3<T1>()
        {
            return default;
        }

        public T1 GetData4<T1>()
        {
            return default;
        }

        public static IGenericData G(T t)
        {
            return RecycleList<GenericsData<T>>.Get().SetData(t);
        }
    }

    public class GenericsData<T1, T2> : IGenericData
    {
        public int Index { get; set; }

        public T1 Data1;
        public T2 Data2;

        public GenericsData<T1, T2> SetData(T1 data1, T2 data2)
        {
            Data1 = data1;
            Data2 = data2;
            return this;
        }

        public void Reset()
        {
            Data1 = default;
            Data2 = default;
        }

        public override string ToString()
        {
            return "<color=#708FFE>Data1:</color> " + Data1 + "<color=#708FFE>Data2:</color> " + Data2;
        }

        public void Recycle()
        {
            RecycleList<GenericsData<T1, T2>>.Recycle(this);
        }

        public T GetData1<T>()
        {
            if (Data1 is T t1)
            {
                return t1;
            }

            return default;
        }

        public T GetData2<T>()
        {
            if (Data2 is T t1)
            {
                return t1;
            }

            return default;
        }

        public T GetData3<T>()
        {
            return default;
        }

        public T GetData4<T>()
        {
            return default;
        }

        public static IGenericData G((T1 t1, T2 t2) tuple)
        {
            return RecycleList<GenericsData<T1, T2>>.Get().SetData(tuple.t1, tuple.t2);
        }
    }

    public class GenericsData<T1, T2, T3> : IGenericData
    {
        public int Index { get; set; }

        public T1 Data1;
        public T2 Data2;
        public T3 Data3;

        public GenericsData<T1, T2, T3> SetData(T1 data1, T2 data2, T3 data3)
        {
            Data1 = data1;
            Data2 = data2;
            Data3 = data3;
            return this;
        }

        public void Reset()
        {
            Data1 = default;
            Data2 = default;
            Data3 = default;
        }

        public override string ToString()
        {
            return "<color=#708FFE>Data1:</color> " + Data1 + "<color=#708FFE>Data2:</color> " + Data2 + "<color=#708FFE>Data3:</color> " + Data3;
        }

        public void Recycle()
        {
            RecycleList<GenericsData<T1, T2, T3>>.Recycle(this);
        }

        public T GetData1<T>()
        {
            if (Data1 is T t1)
            {
                return t1;
            }

            return default;
        }

        public T GetData2<T>()
        {
            if (Data2 is T t1)
            {
                return t1;
            }

            return default;
        }

        public T GetData3<T>()
        {
            if (Data3 is T t1)
            {
                return t1;
            }

            return default;
        }

        public T GetData4<T>()
        {
            return default;
        }

        public static IGenericData G((T1 t1, T2 t2, T3 t3) tuple)
        {
            return RecycleList<GenericsData<T1, T2, T3>>.Get().SetData(tuple.t1, tuple.t2, tuple.t3);
        }
    }

    public class GenericsData<T1, T2, T3, T4> : IGenericData
    {
        public int Index { get; set; }

        public T1 Data1;
        public T2 Data2;
        public T3 Data3;
        public T4 Data4;

        public GenericsData<T1, T2, T3, T4> SetData(T1 data1, T2 data2, T3 data3, T4 data4)
        {
            Data1 = data1;
            Data2 = data2;
            Data3 = data3;
            Data4 = data4;
            return this;
        }

        public void Reset()
        {
            Data1 = default;
            Data2 = default;
            Data3 = default;
            Data4 = default;
        }

        public override string ToString()
        {
            return "<color=#708FFE>Data1:</color> " + Data1 + "<color=#708FFE>Data2:</color> " + Data2 + "<color=#708FFE>Data3:</color> " + Data3 +
                   "<color=#708FFE>Data4:</color> " + Data4;
        }

        public static GenericsData<T1, T2, T3, T4> Get()
        {
            return RecycleList<GenericsData<T1, T2, T3, T4>>.Get();
        }

        public void Recycle()
        {
            RecycleList<GenericsData<T1, T2, T3, T4>>.Recycle(this);
        }

        public T GetData1<T>()
        {
            if (Data1 is T t1)
            {
                return t1;
            }

            return default;
        }

        public T GetData2<T>()
        {
            if (Data2 is T t1)
            {
                return t1;
            }

            return default;
        }

        public T GetData3<T>()
        {
            if (Data3 is T t1)
            {
                return t1;
            }

            return default;
        }

        public T GetData4<T>()
        {
            if (Data4 is T t1)
            {
                return t1;
            }

            return default;
        }

        public static IGenericData G((T1 t1, T2 t2, T3 t3, T4 t4) tuple)
        {
            return RecycleList<GenericsData<T1, T2, T3, T4>>.Get().SetData(tuple.t1, tuple.t2, tuple.t3, tuple.t4);
        }
    }
}