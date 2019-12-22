using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ulee.Device;
using Ulee.Utils;

namespace Ulee.Device.Ajin
{
    public sealed class AxtDioWordModule : AxtObject
    {
        public AxtDioWordModule(int moduleNo = 0, int offset = 0)
        {
            ModuleNo = moduleNo;
            Offset = offset;
        }

        public int ModuleNo { get; private set; }

        public int Offset { get; private set; }
    }

    public class AxtDio : AxtObject
    {
        public AxtDio(EAxtDio type = EAxtDio.DI)
        {
            Type = type;
            TripCode = 0;

            nowValue = null;
            oldValue = null;
            tempValue = null;

            modules = new List<AxtDioWordModule>();

            if (Type == EAxtDio.DI)
                trips = new Dictionary<int, UlTripState>();
            else
                trips = null;

            timers = new Dictionary<int, UlRelayTimer>();
        }

        public EAxtDio Type { get; private set; }

        private UlBinSets nowValue;

        private UlBinSets oldValue;

        private UlBinSets tempValue;

        private List<AxtDioWordModule> modules;

        private Dictionary<int, UlTripState> trips;

        private Dictionary<int, UlRelayTimer> timers;

        public int TripCode { get; private set; }

        public bool this[int index]
        {
            get
            {
                if (nowValue == null)
                {
                    throw new AxtException("Occurred empty DIO exception in AxtDio");
                }
                CheckRange(index);

                bool active = false;

                lock (this)
                {
                    active = nowValue[index];
                }

                return active;
            }

            set
            {
                if (nowValue == null)
                {
                    throw new AxtException("Occurred empty DIO exception in AxtDio");
                }
                if (Type == EAxtDio.DI)
                {
                    throw new AxtException("Occurred writing exception to DI in AxtDio");
                }
                CheckRange(index);

                lock (this)
                {
                    tempValue[index] = value;
                }
            }
        }

        public bool this[string name]
        {
            get
            {
                if (nowValue == null)
                {
                    throw new AxtException("Occurred empty DIO exception in AxtDio");
                }

                bool active = false;

                lock (this)
                {
                    active = nowValue[name];
                }

                return active;
            }

            set
            {
                if (nowValue == null)
                {
                    throw new AxtException("Occurred empty DIO exception in AxtDio");
                }
                if (Type == EAxtDio.DI)
                {
                    throw new AxtException("Occurred writing exception to DI in AxtDio");
                }

                lock (this)
                {
                    tempValue[name] = value;
                }
            }
        }

        public int Count
        {
            get
            {
                if (nowValue == null) return 0;

                return nowValue.Count;
            }
        }

        public bool Changed(int index)
        {
            bool active = false;

            lock (this)
            {
                active = (nowValue[index] != oldValue[index]) ? true : false;
            }

            return active;
        }

        public bool Changed(string name)
        {
            bool active = false;

            lock (this)
            {
                active = (nowValue[name] != oldValue[name]) ? true : false;
            }

            return active;
        }

        public bool RisingEdge(int index)
        {
            bool active = false;

            lock (this)
            {
                active = ((oldValue[index] == false) || (nowValue[index] == true)) ? true : false;
            }

            return active;
        }

        public bool RisingEdge(string name)
        {
            bool active = false;

            lock (this)
            {
                active = ((oldValue[name] == false) || (nowValue[name] == true)) ? true : false;
            }

            return active;
        }

        public bool FallingEdge(int index)
        {
            bool active = false;

            lock (this)
            {
                active = ((oldValue[index] == true) || (nowValue[index] == false)) ? true : false;
            }

            return active;
        }

        public bool FallingEdge(string name)
        {
            bool active = false;

            lock (this)
            {
                active = ((oldValue[name] == true) || (nowValue[name] == false)) ? true : false;
            }

            return active;
        }

        public UlRelayTimer Timer(int index)
        {
            return timers[index];
        }

        public UlRelayTimer Timer(string name)
        {
            return timers[nowValue.TagIndex(name)];
        }

        public bool TimerQ(int index)
        {
            bool active = false;

            lock (this)
            {
                active = timers[index].Q;
            }

            return active;
        }

        public bool TimerQ(string name)
        {
            bool active = false;

            lock (this)
            {
                active = timers[nowValue.TagIndex(name)].Q;
            }

            return active;
        }

        public void AddName(string name)
        {
            nowValue.AddTag(name);
            oldValue.AddTag(name);
            tempValue.AddTag(name);
        }

        public void ClearNames()
        {
            nowValue.ClearTags();
            oldValue.ClearTags();
            tempValue.ClearTags();
        }

        public void AddModule(AxtDioWordModule module)
        {
            modules.Add(module);
        }

        public void AddTrip(int index, UlTripState trip)
        {
            if (Type == EAxtDio.DO)
            {
                throw new AxtException("Occurred adding trip exception to DO in AxtDio");
            }

            CheckRange(index);
            trips.Add(index, trip);
        }

        public void AddTrip(string name, UlTripState trip)
        {
            if (Type == EAxtDio.DO)
            {
                throw new AxtException("Occurred adding trip exception to DO in AxtDio");
            }

            int index = nowValue.TagIndex(name);

            CheckRange(index);
            trips.Add(index, trip);
        }

        public void AddTimer(int index, UlRelayTimer timer)
        {
            CheckRange(index);
            timers.Add(index, timer);
        }

        public void AddTimer(string name, UlRelayTimer timer)
        {
            int index = nowValue.TagIndex(name);

            CheckRange(index);
            timers.Add(index, timer);
        }

        public void Initialize()
        {
            if (modules.Count == 0) return;

            nowValue = new UlBinSets(modules.Count * 2);
            oldValue = new UlBinSets(modules.Count * 2);
            tempValue = new UlBinSets(modules.Count * 2);
        }

        public void Read()
        {
            int i = 0;
            UInt32 value = 0;

            lock (this)
            {
                if (Type == EAxtDio.DI)
                {
                    foreach (AxtDioWordModule module in modules)
                    {
                        i = module.ModuleNo * 2;
                        Validate(CAXD.AxdiReadInportWord(module.ModuleNo, module.Offset, ref value));

                        oldValue.Word(i, nowValue.Word(i));
                        nowValue.Word(i, (UInt16)value);
                    }

                    foreach (KeyValuePair<int, UlTripState> trip in trips)
                    {
                        if (trip.Value.Tripped(nowValue[trip.Key]) == true)
                        {
                            TripCode = trip.Value.Code;
                            break;
                        }
                    }

                    foreach (KeyValuePair<int, UlRelayTimer> timer in timers)
                    {
                        timer.Value.IN = nowValue[timer.Key];
                    }
                }
                else
                {
                    foreach (AxtDioWordModule module in modules)
                    {
                        i = module.ModuleNo * 2;
                        Validate(CAXD.AxdoReadOutportWord(module.ModuleNo, module.Offset, ref value));

                        oldValue.Word(i, nowValue.Word(i));
                        nowValue.Word(i, (UInt16)value);
                        tempValue.Word(i, (UInt16)value);
                    }
                }
            }
        }

        public void Write()
        {
            int i = 0;

            lock (this)
            {
                if (Type == EAxtDio.DO)
                {
                    foreach (AxtDioWordModule module in modules)
                    {
                        i = module.ModuleNo * 2;

                        if (nowValue.Word(i) != tempValue.Word(i))
                        {
                            Validate(CAXD.AxdoWriteOutportWord(module.ModuleNo, module.Offset, tempValue.Word(i)));
                        }
                    }

                    foreach (KeyValuePair<int, UlRelayTimer> timer in timers)
                    {
                        timer.Value.IN = tempValue[timer.Key];
                    }
                }
            }
        }

        private void Validate(UInt32 code)
        {
            base.Validate((AXT_FUNC_RESULT)code, "AxtDio");
        }

        private void CheckRange(int index)
        {
            if (index < 0)
            {
                throw new AxtException("Occurred invalid tag name Exception in AxtDio");
            }
            else if (index >= nowValue.Count)
            {
                throw new AxtException("Occurred over range Exception in AxtDio");
            }
        }
    }

    //public class AxtAioObject
    //{
    //    public EAxtAio Type { get; private set; }

    //    public int Module { get; private set; }

    //    public int Offset { get; private set; }

    //    private UInt16 raw;
    //    public UInt16 Raw
    //    {
    //        get { return raw; }

    //        set
    //        {
    //            Raw = value;
    //            Real = Raw * RealScale + RealOffset;
    //            Real = Real * CalScale + CalOffset;
    //        }
    //    }

    //    public double Real { get; private set; }

    //    public double RealScale { get; set; }

    //    public double RealOffset { get; set; }

    //    public double CalScale { get; set; }

    //    public double CalOffset { get; set; }

    //    private UlTripRange trip;

    //    public bool Tripped
    //    { get { return trip.Tripped(Real); } }

    //    public AxtAioObject(EAxtAio type = EAxtAio.AI, int module = 0, int offset = 0,
    //        double realScale = 0, double realOffset = 0, double calScale = 0, double calOffset = 0,
    //        int tripCode = 0, double tripMin = 0, double tripMax = 0)
    //    {
    //        Type = type;
    //        Module = module;
    //        Offset = offset;

    //        raw = 0;

    //        RealScale = realScale;
    //        RealOffset = realOffset;

    //        CalScale = calScale;
    //        CalOffset = calOffset;

    //        trip = new UlTripRange(tripCode, tripMin, tripMax);
    //    }
    //}
}
