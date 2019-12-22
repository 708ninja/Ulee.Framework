using System;
using System.Collections.Generic;

namespace OxLib.Device.TwinCAT
{
    public class OxTcFunction
    {
        Random rndHandle;
        OxTcAdsClient client;

        private Dictionary<int, OxTcInstruction> instructs;
        public Dictionary<int, OxTcInstruction> Instructs
        {
            get { return instructs; }
        }

        private const int instLength = 16;
        private readonly object criticalLock;

        public OxTcFunction(OxTcAdsClient client)
        {
            this.client = client;

            criticalLock = new object();
            rndHandle = new Random();
            instructs = new Dictionary<int, OxTcInstruction>();
        }

        public int Add(OxTcInstruction inst)
        {
            if (instructs.Count >= instLength)
            {
                throw new Exception("Instruction counter overflow error in OxTcFunction::Add");
            }

            int handle = rndHandle.Next();

            lock (criticalLock)
            {
                instructs.Add(handle, inst);
            }

            return handle;
        }

        public void Remove(int handle)
        {
            Contain(handle);

            lock (criticalLock)
            {
                instructs.Remove(handle);
            }
        }

        public void Clear()
        {
            lock (criticalLock)
            {
                instructs.Clear();
            }
        }

        public OxTcInstruction GetInstruction(int handle)
        {
            Contain(handle);

            OxTcInstruction inst;
            lock (criticalLock)
            {
                inst = instructs[handle];
            }

            return inst;
        }

        public bool IsInstructionRunning(int handle)
        {
            Contain(handle);

            bool active;
            lock (criticalLock)
            {
                active = instructs[handle].Active;
            }

            return active;
        }

        public int GetInstructionState(int handle)
        {
            Contain(handle);

            int state;
            lock (criticalLock)
            {
                state = instructs[handle].State;
            }

            return state;
        }

        public Int16 GetInstructionNo(int handle)
        {
            Contain(handle);

            Int16 instNo;
            lock (criticalLock)
            {
                instNo = instructs[handle].InstructNo;
            }

            return instNo;
        }

        public OxTcArguments GetInstructionArgs(int handle)
        {
            Contain(handle);

            OxTcArguments args;
            lock (criticalLock)
            {
                args = instructs[handle].Args;
            }

            return args;
        }

        public Int16 GetInstructionIndex(int handle)
        {
            Contain(handle);

            Int16 index;
            lock (criticalLock)
            {
                index = instructs[handle].Index;
            }

            return index;
        }

        private void Contain(int handle)
        {
            bool isContain;

            lock (criticalLock)
            {
                isContain = instructs.ContainsKey(handle);
            }

            if (isContain == false)
            {
                throw new Exception("Invalid handle error in OxTcFunction::Contain");
            }
        }
    }

    public class OxTcInstruction
    {
        private Int16 instructNo;
        public Int16 InstructNo
        {
            get { return instructNo; }
            set { instructNo = value; }
        }

        private OxTcArguments args;
        public OxTcArguments Args
        {
            get { return args; }
            set { args = value; }
        }

        private Int16 index;
        public Int16 Index
        {
            get { return index; }
            set { index = value; }
        }

        private bool active;
        public bool Active
        {
            get { return active; }
            set { active = value; }
        }

        private Int16 state;
        public Int16 State
        {
            get { return state; }
            set { state = value; }
        }

        public OxTcInstruction(int length = 8)
        {
            args = new OxTcArguments(length);
            Clear();
        }

        public void Clear()
        {
            instructNo = 0;
            index = 0;
            active = false;
            state = 0;

            for (int i=0; i<args.Length; i++)
            {
                args.Bools[i] = false;
                args.Bytes[i] = 0;
                args.Int16s[i] = 0;
                args.Int32s[i] = 0;
                args.Int64s[i] = 0;
                args.Floats[i] = 0;
                args.Doubles[i] = 0;
            }
        }
    }

    public class OxTcArguments
    {
        public int Length;

        public bool[] Bools;
        public byte[] Bytes;
        public Int16[] Int16s;
        public Int32[] Int32s;
        public Int64[] Int64s;
        public float[] Floats;
        public double[] Doubles;

        public OxTcArguments(int length)
        {
            Length = length;

            Bools = new bool[length];
            Bytes = new byte[length];
            Int16s = new Int16[length];
            Int32s = new Int32[length];
            Int64s = new Int64[length];
            Floats = new float[length];
            Doubles = new double[length];
        }
    }
}
