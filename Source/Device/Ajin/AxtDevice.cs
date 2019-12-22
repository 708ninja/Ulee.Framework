using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ulee.Device.Ajin
{
    public class AxtDevice : AxtObject
    {
        public AxtDevice()
        {
            DI = new AxtDio(EAxtDio.DI);
            DO = new AxtDio(EAxtDio.DO);
            Axis = new AxtAxis();
        }

        public AxtDio DI { get; private set; }

        public AxtDio DO { get; private set; }

        public AxtAxis Axis { get; private set; }

        public void Initialize()
        {
            // Ajinextek 시스템 초기화 되었는가?
            if (CAXL.AxlIsOpened() != 0) return;

            // Ajinextek 시스템 초기화
            Validate(CAXL.AxlOpen(0));

            InitializeDIO();
            InitializeAxes();
        }

        public void Close()
        {
            // Ajinextek 시스템 초기화 되었는가?
            if (CAXL.AxlIsOpened() != 0)
            {
                CAXL.AxlClose();
            }
        }

        private void InitializeDIO()
        {
            UInt32 retValue = 0;

            // DIO 모듈 존재여부 확인
            Validate(CAXD.AxdInfoIsDIOModule(ref retValue));

            if (retValue == (UInt32)AXT_EXISTENCE.STATUS_EXIST)
            {
                int moduleCount = 0;

                Validate(CAXD.AxdInfoGetModuleCount(ref moduleCount));

                for (int i = 0; i < moduleCount; i++)
                {
                    int channelCount = 0;

                    // DI 채널 개수 확인
                    Validate(CAXD.AxdInfoGetInputCount(i, ref channelCount));

                    if (channelCount > 0)
                    {
                        for (int j = 0; j < channelCount / 16; j++)
                        {
                            AxtDioWordModule module = new AxtDioWordModule(i, j);
                            DI.AddModule(module);
                        }
                    }

                    // DO 채널 개수 확인
                    Validate(CAXD.AxdInfoGetOutputCount(i, ref channelCount));

                    if (channelCount > 0)
                    {
                        for (int j = 0; j < channelCount / 16; j++)
                        {
                            AxtDioWordModule module = new AxtDioWordModule(i, j);
                            DO.AddModule(module);
                        }
                    }
                }

                DI.Initialize();
                DO.Initialize();
                Read();
            }
        }

        private void InitializeAxes()
        {
            UInt32 retValue = 0;

            // Motion axis 모듈 존재여부 확인
            Validate(CAXM.AxmInfoIsMotionModule(ref retValue));

            if (retValue == (UInt32)AXT_EXISTENCE.STATUS_EXIST)
            {
                int moduleCount = 0;

                Validate(CAXM.AxmInfoGetAxisCount(ref moduleCount));

                if (moduleCount > 0)
                {
                    for (int i=0; i<moduleCount; i++)
                    {
                        Axis.AddModule(new AxtAxisModule(i, "", Axis));
                    }

                    Axis.Reset();
                }
            }
        }

        private void Validate(UInt32 code)
        {
            base.Validate((AXT_FUNC_RESULT)code, "AxtDevice");
        }

        public void Read()
        {
            DI.Read();
            DO.Read();
        }

        public void Write()
        {
            DO.Write();
        }
    }
}
