//------------------------------------------------------------------------------
// Copyright (C) 2018 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2018/03/28
// Language    : Visual Studio 2017 C# for .NET 4.6.1
// Description : TwinCAT PLC Procedure and Instruction Class
//------------------------------------------------------------------------------
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

using TwinCAT.Ads;

namespace Ulee.Device.Connection.TwinCAT
{
    public class UlTcProcedure
    {
        private Random rndHandle;

        private string name;
        private UlTcAdsClient client;
        private UlTcInstruction inst;

        private Dictionary<int, UlTcInstruction> instructs;
        public Dictionary<int, UlTcInstruction> Instructs
        {
            get { return instructs; }
        }

        private int instLength;
        private readonly object dataLock;

        public UlTcProcedure(string name, UlTcAdsClient client, int length = 16)
        {
            this.name = name;
            this.client = client;
            this.instLength = length;

            dataLock = new object();
            rndHandle = new Random();

            inst = new UlTcInstruction();
            instructs = new Dictionary<int, UlTcInstruction>();
        }

        public int Add(UlTcInstruction inst, long waiting=1000)
        {
            Stopwatch sw = new Stopwatch();

            if (instructs.Count >= instLength)
            {
                throw new Exception("Instruction counter overflow error in UlTcFunction::Add");
            }

            sw.Start();
            while (ContainsIndex(inst.Index) == true)
            {
                if (sw.ElapsedMilliseconds > waiting)
                {
                    throw new Exception("Duplicated instruction index error in UlTcFunction::Add");
                }

                Thread.Sleep(1);
            }

            inst.Handle = rndHandle.Next();

            lock (dataLock)
            {
                instructs.Add(inst.Handle, inst);
            }

            return inst.Handle;
        }

        public void Remove(int handle)
        {
            ContainsHandle(handle);

            lock (dataLock)
            {
                instructs.Remove(handle);
            }
        }

        public void Clear()
        {
            lock (dataLock)
            {
                instructs.Clear();
            }
        }

        public void Read()
        {
            string str;

            for (int i = 0; i < instLength; i++)
            {
                str = string.Format("{0}[{1}]", name, i);
                client.Read(str, DoReadTcInstructions, UlTcInstruction.ParamLength);

                if (inst.Handle > -1)
                {
                    client.Read(str + ".args", DoReadTcArgs, UlTcInstruction.ArgsLength);

                    ContainsHandle(inst.Handle);

                    lock (dataLock)
                    {
                        instructs[inst.Handle].Index = inst.Index;
                        instructs[inst.Handle].Step = inst.Step;
                        instructs[inst.Handle].Active = inst.Active;
                        instructs[inst.Handle].State = inst.State;

                        for (int j = 0; j < 8; j++)
                        {
                            instructs[inst.Handle].Args.Bools[j] = inst.Args.Bools[j];
                            instructs[inst.Handle].Args.Bytes[j] = inst.Args.Bytes[j];
                            instructs[inst.Handle].Args.Int16s[j] = inst.Args.Int16s[j];
                            instructs[inst.Handle].Args.Int32s[j] = inst.Args.Int32s[j];
                            instructs[inst.Handle].Args.Floats[j] = inst.Args.Floats[j];
                            instructs[inst.Handle].Args.Doubles[j] = inst.Args.Doubles[j];
                        }
                    }
                }
            }
        }

        public void Write()
        {
            int i, j, k;
            string str;

            i = 0;
            foreach (KeyValuePair<int, UlTcInstruction> dic in instructs)
            {
                str = string.Format("{0}[{1}]", name, i);

                inst.Handle = dic.Value.Handle;
                inst.Index = dic.Value.Index;
                inst.Step = dic.Value.Step;
                inst.Active = dic.Value.Active;
                inst.State = dic.Value.State;

                for (j = 0; j < 8; j++)
                {
                    inst.Args.Bools[j] = dic.Value.Args.Bools[j];
                    inst.Args.Bytes[j] = dic.Value.Args.Bytes[j];
                    inst.Args.Int16s[j] = dic.Value.Args.Int16s[j];
                    inst.Args.Int32s[j] = dic.Value.Args.Int32s[j];
                    inst.Args.Floats[j] = dic.Value.Args.Floats[j];
                    inst.Args.Doubles[j] = dic.Value.Args.Doubles[j];
                }

                client.Write(str, DoWriteTcInstructions);
                client.Write(str + ".args", DoWriteTcArgs);

                i++;
            }

            for (j = i; j < instLength; j++)
            {
                str = string.Format("{0}[{1}]", name, j);

                inst.Handle = -1;
                inst.Index = 0;
                inst.Step = 0;
                inst.Active = false;
                inst.State = 0;

                for (k = 0; k < 8; k++)
                {
                    inst.Args.Bools[k] = false;
                    inst.Args.Bytes[k] = 0;
                    inst.Args.Int16s[k] = 0;
                    inst.Args.Int32s[k] = 0;
                    inst.Args.Floats[k] = 0;
                    inst.Args.Doubles[k] = 0;
                }

                client.Write(str, DoWriteTcInstructions);
                client.Write(str + ".args", DoWriteTcArgs);
            }
        }

        private void DoReadTcInstructions(AdsBinaryReader reader)
        {
            inst.Handle = reader.ReadInt32();
            inst.Index = reader.ReadInt16();
            inst.Step = reader.ReadInt16();
            inst.Active = reader.ReadBoolean();
            inst.State = reader.ReadInt16();
        }

        private void DoReadTcArgs(AdsBinaryReader reader)
        {

            for (int i = 0; i < 8; i++) inst.Args.Bools[i] = reader.ReadBoolean();
            for (int i = 0; i < 8; i++) inst.Args.Bytes[i] = reader.ReadByte();
            for (int i = 0; i < 8; i++) inst.Args.Int16s[i] = reader.ReadInt16();
            for (int i = 0; i < 8; i++) inst.Args.Int32s[i] = reader.ReadInt32();
            for (int i = 0; i < 8; i++) inst.Args.Floats[i] = reader.ReadSingle();
            for (int i = 0; i < 8; i++) inst.Args.Doubles[i] = reader.ReadDouble();
        }

        private void DoWriteTcInstructions(AdsBinaryWriter writer)
        {
            writer.Write(inst.Handle);
            writer.Write(inst.Index);
            writer.Write(inst.Step);
            writer.Write(inst.Active);
            writer.Write(inst.State);
        }

        private void DoWriteTcArgs(AdsBinaryWriter writer)
        {
            int i;

            for (i = 0; i < 8; i++) writer.Write(inst.Args.Bools[i]);
            for (i = 0; i < 8; i++) writer.Write(inst.Args.Bytes[i]);
            for (i = 0; i < 8; i++) writer.Write(inst.Args.Int16s[i]);
            for (i = 0; i < 8; i++) writer.Write(inst.Args.Int32s[i]);
            for (i = 0; i < 8; i++) writer.Write(inst.Args.Floats[i]);
            for (i = 0; i < 8; i++) writer.Write(inst.Args.Doubles[i]);
        }

        public UlTcInstruction GetInstruction(int handle)
        {
            ContainsHandle(handle);

            UlTcInstruction inst;
            lock (dataLock)
            {
                inst = instructs[handle];
            }

            return inst;
        }

        public bool IsInstructionRunning(int handle)
        {
            ContainsHandle(handle);

            bool active;
            lock (dataLock)
            {
                active = instructs[handle].Active;
            }

            return active;
        }

        public int GetInstructionState(int handle)
        {
            ContainsHandle(handle);

            int state;
            lock (dataLock)
            {
                state = instructs[handle].State;
            }

            return state;
        }

        public Int16 GetInstructionIndex(int handle)
        {
            ContainsHandle(handle);

            Int16 number;
            lock (dataLock)
            {
                number = instructs[handle].Index;
            }

            return number;
        }

        public UlTcArguments GetInstructionArgs(int handle)
        {
            ContainsHandle(handle);

            UlTcArguments args;
            lock (dataLock)
            {
                args = instructs[handle].Args;
            }

            return args;
        }

        public Int16 GetInstructionStep(int handle)
        {
            ContainsHandle(handle);

            Int16 step;
            lock (dataLock)
            {
                step = instructs[handle].Index;
            }

            return step;
        }

        private bool ContainsIndex(int index)
        {
            bool isContain = false;

            lock (dataLock)
            {
                foreach (KeyValuePair<int, UlTcInstruction> dic in instructs)
                {
                    if (dic.Value.Index == index)
                    {
                        isContain = true;
                        break;
                    }
                }
            }

            return isContain;
        }

        private void ContainsHandle(int handle)
        {
            bool isContain;

            lock (dataLock)
            {
                isContain = instructs.ContainsKey(handle);
            }

            if (isContain == false)
            {
                throw new Exception("Invalid handle error in UlTcFunction::Contain");
            }
        }
    }

    public class UlTcInstruction
    {
        private int handle;
        public int Handle
        {
            get { return handle; }
            set { handle = value; }
        }

        private Int16 index;
        public Int16 Index
        {
            get { return index; }
            set { index = value; }
        }

        private Int16 step;
        public Int16 Step
        {
            get { return step; }
            set { step = value; }
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

        private UlTcArguments args;
        public UlTcArguments Args
        {
            get { return args; }
            set { args = value; }
        }

        public const int ParamLength = 11;
        public const int ArgsLength = 160;

        public UlTcInstruction()
        {
            args = new UlTcArguments(8);
            Clear();
        }

        public void Clear()
        {
            handle = -1;
            index = 0;
            step = 0;
            active = false;
            state = 0;

            for (int i = 0; i < args.Length; i++)
            {
                args.Bools[i] = false;
                args.Bytes[i] = 0;
                args.Int16s[i] = 0;
                args.Int32s[i] = 0;
                args.Floats[i] = 0;
                args.Doubles[i] = 0;
            }
        }
    }

    public class UlTcArguments
    {
        public int Length;

        public bool[] Bools;
        public byte[] Bytes;
        public Int16[] Int16s;
        public Int32[] Int32s;
        public float[] Floats;
        public double[] Doubles;

        public UlTcArguments(int length)
        {
            Length = length;

            Bools = new bool[length];
            Bytes = new byte[length];
            Int16s = new Int16[length];
            Int32s = new Int32[length];
            Floats = new float[length];
            Doubles = new double[length];
        }
    }
}
