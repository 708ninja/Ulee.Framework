using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Ulee.Utils;

namespace Ulee.Device.Ajin
{
    public class AxtAxisModule: AxtObject
    {
        public AxtAxisModule(int index=0, string name="", object lockObject=null)
        {

            Index = index;
            Name = name;
            execLock = (lockObject == null) ? this : lockObject;

            HiVelocity = 0;
            HiAcceleration = 0;
            HiDeceleration = 0;

            LowVelocity = 0;
            LowAcceleration = 0;
            LowDeceleration = 0;
        }

        private object execLock;

        // 서보 구동축 번호
        public int Index { get; set; }

        // 서보 구동축 이름
        public string Name { get; set; }

        // 상위 속도
        public double HiVelocity { get; set; }

        // 상위 가속 속도
        public double HiAcceleration { get; set; }

        // 상위 감속 속도
        public double HiDeceleration { get; set; }

        // 하위 속도
        public double LowVelocity { get; set; }

        // 하위 가속 속도
        public double LowAcceleration { get; set; }

        // 하위 감속 속도
        public double LowDeceleration { get; set; }

        // 서보 On 신호 - AxmSignalIsServoOn
        public bool Active
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmSignalIsServoOn(Index, ref value));

                return (value == 0) ? false : true;
            }

            set
            {
                Validate(CAXM.AxmSignalServoOn(Index, (value ? (UInt32)1 : (UInt32)0)));
            }
        }

        // 서보 Reset 신호 - AxmSignalServoAlarmReset
        public bool Reset
        {
            set
            {
                Validate(CAXM.AxmSignalServoAlarmReset(Index, (value ? (UInt32)1 : (UInt32)0)));
            }
        }

        // 서보 Alarm 신호 - AxmSignalReadServoAlarm
        public bool Alarm
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmSignalReadServoAlarm(Index, ref value));

                return (value == 0) ? false : true;
            }
        }

        // Emergency Stop 신호 - AxmSignalReadStop
        public bool EStop
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmSignalReadStop(Index, ref value));

                return (value == 0) ? false : true;
            }
        }

        // 서보 On Level 신호 - AxmSignalGetServoOnLevel
        public AXT_MOTION_LEVEL_MODE ActiveLevel
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmSignalGetServoOnLevel(Index, ref value));

                return (AXT_MOTION_LEVEL_MODE)value;
            }

            set
            {
                Validate(CAXM.AxmSignalSetServoOnLevel(Index, (UInt32)value));
            }
        }

        // 서보 Reset Level 신호 - AxmSignalGetServoAlarmResetLevel
        public AXT_MOTION_LEVEL_MODE ResetLevel
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmSignalGetServoAlarmResetLevel(Index, ref value));

                return (AXT_MOTION_LEVEL_MODE)value;
            }

            set
            {
                Validate(CAXM.AxmSignalSetServoAlarmResetLevel(Index, (UInt32)value));
            }
        }

        // 서보 Alarm Level 신호 - AxmSignalGetServoAlarm
        public AXT_MOTION_LEVEL_MODE AlarmLevel
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmSignalGetServoAlarm(Index, ref value));

                return (AXT_MOTION_LEVEL_MODE)value;
            }

            set
            {
                Validate(CAXM.AxmSignalSetServoAlarm(Index, (UInt32)value));
            }
        }

        // Home Level 신호 - AxmHomeGetSignalLevel
        public AXT_MOTION_LEVEL_MODE HomeLevel
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmHomeGetSignalLevel(Index, ref value));

                return (AXT_MOTION_LEVEL_MODE)value;
            }

            set
            {
                Validate(CAXM.AxmHomeSetSignalLevel(Index, (UInt32)value));
            }
        }

        // Encoder Z Phase Level 신호 - AxmSignalGetZphaseLevel
        public AXT_MOTION_LEVEL_MODE EncoderZLevel
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmSignalGetZphaseLevel(Index, ref value));

                return (AXT_MOTION_LEVEL_MODE)value;
            }

            set
            {
                Validate(CAXM.AxmSignalSetZphaseLevel(Index, (UInt32)value));
            }
        }

        // 서보 구동중 신호 - AxmStatusReadInMotion
        public bool Busy
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmStatusReadInMotion(Index, ref value));

                return (value == 0) ? false : true;
            }
        }

        // 서보 구동상태 신호 - AxmStatusReadMotion
        public UInt32 MotionState
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmStatusReadMotion(Index, ref value));

                return value;
            }
        }

        // 외부센서 및 모터관련신호 - AxmStatusReadMechanical
        public UInt32 SignalState
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmStatusReadMechanical(Index, ref value));

                return value;
            }
        }

        // 원점 검출 동작 상태 - AxmHomeGetResult
        public AXT_MOTION_HOME_RESULT HomeState
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmHomeGetResult(Index, ref value));

                return (AXT_MOTION_HOME_RESULT)value;
            }

            set
            {
                Validate(CAXM.AxmHomeSetResult(Index, (UInt32)value));
            }
        }

        // 서보 현재 위치 - AxmStatusGetActPos
        public double ActualPosition
        {
            get
            {
                double value = 0;
                Validate(CAXM.AxmStatusGetActPos(Index, ref value));

                return value;
            }

            set
            {
                Validate(CAXM.AxmStatusSetActPos(Index, value));
            }
        }

        // 서보 지령 위치 - AxmStatusGetCmdPos
        public double CommandPosition
        {
            get
            {
                double value = 0;
                Validate(CAXM.AxmStatusGetCmdPos(Index, ref value));

                return value;
            }

            set
            {
                Validate(CAXM.AxmStatusSetCmdPos(Index, value));
            }
        }

        // 서보 위치 편차(지령위치-현재위치) - AxmStatusReadPosError
        public double PositionDeviation
        {
            get
            {
                double value = 0;
                Validate(CAXM.AxmStatusReadPosError(Index, ref value));

                return value;
            }
        }

        // 구동 Pulse Count : PCI-N404/804는 구동중에만 유효함 - AxmStatusReadDrivePulseCount
        public int PulseCount
        {
            get
            {
                int value = 0;
                Validate(CAXM.AxmStatusReadDrivePulseCount(Index, ref value));

                return value;
            }
        }

        // 이동거리 : PCI-N404/804는 구동중에만 유효함 - AxmStatusReadDriveDistance
        public double Distance
        {
            get
            {
                double value = 0;
                Validate(CAXM.AxmStatusReadDriveDistance(Index, ref value));

                return value;
            }
        }

        public double Velocity
        {
            get
            {
                double value = 0;
                Validate(CAXM.AxmStatusReadVel(Index, ref value));

                return value;
            }
        }

        // (+)Limit 신호 - AxmSignalReadLimit
        public bool PositiveLimit
        {
            get
            {
                UInt32 value1 = 0;
                UInt32 value2 = 0;
                CAXM.AxmSignalReadLimit(Index, ref value1, ref value2);

                return (value1 == 0) ? false : true;
            }
        }

        // (-)Limit 신호 - AxmSignalReadLimit
        public bool NegativeLimit
        {
            get
            {
                UInt32 value1 = 0;
                UInt32 value2 = 0;
                CAXM.AxmSignalReadLimit(Index, ref value1, ref value2);

                return (value2 == 0) ? false : true;
            }
        }

        // 구동중 구동 속도 변경
        public double OverrideVelocity
        {
            set
            {
                lock (execLock)
                {
                    Validate(CAXM.AxmOverrideSetMaxVel(Index, value));
                    Validate(CAXM.AxmOverrideVel(Index, value));
                }
            }
        }

        // 최대 구동 속도 제한 - AxmMotSetMaxVel
        public double MaximumVelocity
        {
            get
            {
                double value = 0;
                Validate(CAXM.AxmMotGetMaxVel(Index, ref value));

                return value;
            }

            set
            {
                Validate(CAXM.AxmMotSetMaxVel(Index, value));
            }
        }

        // 최소 구동 속도 제한 - AxmMotSetMinVel
        public double MinimumVelocity
        {
            get
            {
                double value = 0;
                Validate(CAXM.AxmMotGetMinVel(Index, ref value));

                return value;
            }

            set
            {
                Validate(CAXM.AxmMotSetMinVel(Index, value));
            }
        }

        // 가감속도 단위 - AxmMotSetAccelUnit
        // UNIT_SEC2(0) - unit/sec2
        // SEC(1)       - sec
        public AXT_MOTION_ACC_UNIT AccelerationUnit
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmMotGetAccelUnit(Index, ref value));

                return (AXT_MOTION_ACC_UNIT)value;
            }

            set
            {
                Validate(CAXM.AxmMotSetAccelUnit(Index, (UInt32)value));
            }
        }

        // S-Curve Profile 가속시 S-Curve 구동범위 - AxmMotSetAccelJerk
        // 0 ~ 100[%]
        public double AccelerationJerk
        {
            get
            {
                double value = 0;
                Validate(CAXM.AxmMotGetAccelJerk(Index, ref value));

                return value;
            }

            set
            {
                Validate(CAXM.AxmMotSetAccelJerk(Index, value));
            }
        }


        // S-Curve Profile 감속시 S-Curve 구동범위 - AxmMotSetDecelJerk
        // 0 ~ 100[%]
        public double DecelerationJerk
        {
            get
            {
                double value = 0;
                Validate(CAXM.AxmMotGetDecelJerk(Index, ref value));

                return value;
            }

            set
            {
                Validate(CAXM.AxmMotSetDecelJerk(Index, value));
            }
        }

        // 가감속 Profile - AxmMotSetProfileMode
        // SYM_TRAPEZOIDE_MODE(0)  - 대칭 사다리꼴
        // ASYM_TRAPEZOIDE_MODE(1) - 비대칭 사다리꼴
        // QUASI_S_CURVE_MODE(2)   - 대칭 Quasi-S Curve(PCI-N404/804 지원안함)
        // SYM_S_CURVE_MODE(3)     - 대칭 S-Curve
        // ASYM_S_CURVE_MODE(4)    - 비대칭 S-Curve
        public AXT_MOTION_PROFILE_MODE Profile
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmMotGetProfileMode(Index, ref value));

                return (AXT_MOTION_PROFILE_MODE)value;
            }

            set
            {
                Validate(CAXM.AxmMotSetProfileMode(Index, (UInt32)value));
            }
        }

        // 좌표계 설정 - AxmMotSetAbsRelMode
        // POS_ABS_MODE(0) - 절대좌표계
        // POS_REL_MODE(1) - 상대좌표계
        public AXT_MOTION_ABSREL Coordinate
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmMotGetAbsRelMode(Index, ref value));

                return (AXT_MOTION_ABSREL)value;
            }

            set
            {
                Validate(CAXM.AxmMotSetAbsRelMode(Index, (UInt32)value));
            }
        }

        // 구동 Pulse Type - AxmMotSetPulseOutMethod
        // OneHighLowHigh(0)  - 1펄스 방식, PULSE(Active High), 정방향(DIR=Low)  역방향(DIR=High)
        // OneHighHighLow(1)  - 1펄스 방식, PULSE(Active High), 정방향(DIR=High) 역방향(DIR=Low)
        // OneLowLowHigh(2)   - 1펄스 방식, PULSE(Active Low),  정방향(DIR=Low)  역방향(DIR=High)
        // OneLowHighLow(3)   - 1펄스 방식, PULSE(Active Low),  정방향(DIR=High) 역방향(DIR=Low)
        // TwoCcwCwHigh(4)    - 2펄스 방식, PULSE(CCW:역방향),  DIR(CW:정방향),  Active High
        // TwoCcwCwLow(5)     - 2펄스 방식, PULSE(CCW:역방향),  DIR(CW:정방향),  Active Low
        // TwoCwCcwHigh(6)    - 2펄스 방식, PULSE(CW:정방향),   DIR(CCW:역방향), Active High
        // TwoCwCcwLow(7)     - 2펄스 방식, PULSE(CW:정방향),   DIR(CCW:역방향), Active Low
        // TwoPhase(8)        - 2상(90' 위상차), PULSE lead DIR(CW: 정방향), PULSE lag DIR(CCW:역방향) SMC-2V03은 사용못함
        // TwoPhaseReverse(9) - 2상(90' 위상차), PULSE lead DIR(CCW: 정방향), PULSE lag DIR(CW:역방향) SMC-2V03은 사용못함
        public AXT_MOTION_PULSE_OUTPUT PulseType
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmMotGetPulseOutMethod(Index, ref value));

                return (AXT_MOTION_PULSE_OUTPUT)value;
            }

            set
            {
                Validate(CAXM.AxmMotSetPulseOutMethod(Index, (UInt32)value));
            }
        }

        // 1단위당 출력 Pulse 개수 - AxmMotSetMoveUnitPerPulse
        // 모터 1회전 Pulse = 100000, 1회전시 볼스쿠루 전진거리 = 10mm 일때 거리 지정 단위를 1um로 설정하려면
        // PulseUnit = 100000 / (0.01(10mm) / 0.000001(1um)) = 10
        public int UnitPerPulse
        {
            get
            {
                double unit = 0;
                int count = 0;
                Validate(CAXM.AxmMotGetMoveUnitPerPulse(Index, ref unit, ref count));

                return count;
            }

            set
            {
                Validate(CAXM.AxmMotSetMoveUnitPerPulse(Index, 1, value));
            }
        }

        // 감속 시작 잔여 Pulse 개수(DecelerationType이 ResetPulse일때 유효) - AxmMotSetRemainPulse
        public UInt32 RemainPulse
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmMotGetRemainPulse(Index, ref value));

                return value;
            }

            set
            {
                Validate(CAXM.AxmMotSetRemainPulse(Index, (UInt32)value));
            }
        }

        // Encoder 입력 체배 - AxmMotSetEncInputMethod
        // ObverseUpDownMode(0) - 정방향 Up/Down
        // ObverseSqr1Mode(1)   - 정방향 1체배
        // ObverseSqr2Mode(2)   - 정방향 2체배
        // ObverseSqr4Mode(3)   - 정방향 4체배
        // ReverseUpDownMode(4) - 역방향 Up/Down
        // ReverseSqr1Mode(5)   - 역방향 1체배
        // ReverseSqr2Mode(6)   - 역방향 2체배
        // ReverseSqr4Mode(7)   - 역방향 4체배
        public AXT_MOTION_EXTERNAL_COUNTER_INPUT EncoderType
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmMotGetEncInputMethod(Index, ref value));

                return (AXT_MOTION_EXTERNAL_COUNTER_INPUT)value;
            }

            set
            {
                Validate(CAXM.AxmMotSetEncInputMethod(Index, (UInt32)value));
            }
        }

        // 감속 시작위치 검출 방식 - AxmMotSetDecelMode
        // AutoDetect(0) - 자동 검출 방식
        // ResetPulse(1) - 나머지 검출 방식
        public AXT_MOTION_DETECT_DOWN_START_POINT DecelerationType
        {
            get
            {
                UInt32 value = 0;
                Validate(CAXM.AxmMotGetDecelMode(Index, ref value));

                return (AXT_MOTION_DETECT_DOWN_START_POINT)value;
            }

            set
            {
                Validate(CAXM.AxmMotSetDecelMode(Index, (UInt32)value));
            }
        }

        // Limit 신호 설정 - AxmSignalSetLimit
        // stopMode(정지방법): EMERGENCY_STOP(0)-급정지, SLOWDOWN_STOP(1)-감속정지
        // posLevel(+Limit)  : LOW(0)-B접점, HIGH(1)-A접점, UNUSED(2)-사용안함, USED(3)-현상태유지
        // negLevel(-Limit)  : LOW(0)-B접점, HIGH(1)-A접점, UNUSED(2)-사용안함, USED(3)-현상태유지
        public void SetSignalLimit(
            AXT_MOTION_STOPMODE stopMode, AXT_MOTION_LEVEL_MODE posLevel, AXT_MOTION_LEVEL_MODE negLevel)
        {
            Validate(CAXM.AxmSignalSetLimit(Index, (UInt32)stopMode, (UInt32)posLevel, (UInt32)negLevel));
        }

        // Soft Limit 신호 설정 - AxmSignalSetSoftLimit
        // use(사용유무)       :  DISABLE(0), ENABLE(1)
        // stopMode(정지방법)  : EMERGENCY_STOP(0)-급정지, SLOWDOWN_STOP(1)-감속정지
        // selection(비교위치) : COMMAND(0)-목표위치(Command Position), ACTUAL(1)-실제위치(Actual Position)
        // posLimitPos(+Limit위치)
        // negLimitPos(-Limit위치)
        public void SetSoftSignalLimit(AXT_USE use, AXT_MOTION_STOPMODE stopMode,
            AXT_MOTION_SELECTION selection, double posLimitPos, double negLimitPos)
        {
            Validate(CAXM.AxmSignalSetSoftLimit(Index, (UInt32)use,
                (UInt32)stopMode, (UInt32)selection, posLimitPos, negLimitPos));
        }

        // 원점 검출 스탭별 속도 설정 - AxmHomeSetVel
        // vel1   : 1단계 고속 원점 검출 속도-원점 검출 후 감속정지
        // vel2   : 2단계 원점 검출 속도-1단계와 반대방향으로 원점 검출 후 감속정지
        // vel3   : 3단계 원점 검출 속도-2단계와 반대방향으로 원점 Up Edge 신호 검출 후 급정지
        // vel4   : 4단계 Encoder Z상 Up/Down Edge 검출 속도-Z상 검출 사용시 유효함
        // accel1 : 1단계 고속 원점 검출 가감속도
        // accel2 : 2단계 이후 원점 검출 가감속도
        public void SetHomeSeekVelocity(
            double vel1, double vel2, double vel3, double vel4, double accel1, double accel2)
        {
            Validate(CAXM.AxmHomeSetVel(Index, vel1, vel2, vel3, vel4, accel1, accel2));
        }

        // 원점 검출 방법 설정 - AxmHomeSetMethod
        // dir(원점검색 진행방향) : DIR_CCW(0)-반시계방향, DIR_CW(1)-시계방향
        // detect(검출Signal)    : PosEndLimit(0)-(+)Limit신호, NegEndLimit(1)-(-)Limit신호, HomeSensor(4)-Home신호
        // zPhase(Z상검출)       : DISABLE(0)-사용안함, (1)-(+)방향검출 , (2)-(-)방향검출
        // clearTime             : 원점 검색 Encoder 값 Set하기 위한 대기시간 
        // offset                : 원점검출후 이동거리.
        public void SetHomeSeekType(
            AXT_MOTION_MOVE_DIR dir, 
            AXT_MOTION_HOME_DETECT detect, 
            UInt32 zPhase, double clearTime, double offset)
        {
            Validate(CAXM.AxmHomeSetMethod(Index, (int)dir, (UInt32)detect, zPhase, clearTime, offset));
        }

        // 비상정지 신호 설정 - AxmSignalSetStop
        // stopMode(정지방법) : EMERGENCY_STOP(0)-급정지, SLOWDOWN_STOP(1)-감속정지(PCI-N404/804 지원안함)
        // level(+Limit)      : LOW(0)-B접점, HIGH(1)-A접점, UNUSED(2)-사용안함, USED(3)-현상태유지
        public void SetEStopLevel(AXT_MOTION_STOPMODE mode, AXT_MOTION_LEVEL_MODE level)
        {
            Validate(CAXM.AxmSignalSetStop(Index, (UInt32)mode, (UInt32)level));
        }

        // 원점 검출 구동 - AxmHomeSetStart
        public void HomeSeek()
        {
            lock (execLock)
            {
                Validate(CAXM.AxmHomeSetStart(Index));
            }
        }

        // 신호 검출 구동 - AxmMoveSignalSearch
        // detect(검출신호)
        //		- PosEndLimit(0) - (+)Elm(End limit) +방향 리미트 센서 신호
        //		- NegEndLimit(1) - (-)Elm(End limit) -방향 리미트 센서 신호
        //		- PosSloLimit(2) - (+)Slm(Slow Down limit) 신호 - 사용하지 않음
        //		- NegSloLimit(3) - (-)Slm(Slow Down limit) 신호 - 사용하지 않음
        //		- HomeSensor(4)  - IN0(ORG) 원점 센서 신호
        //		- EncodZPhase(5) - IN1(Z상) Encoder Z상 신호
        //		- UniInput02(6)  - IN2(범용) 범용 입력 2번 신호
        //		- UniInput03(7)  - IN3(범용) 범용 입력 3번 신호
        // edge(검출Edge) : SIGNAL_DOWN_EDGE(0)-다운에지, SIGNAL_UP_EDGE(1)-업에지
        // mode(정지방법)  : EMERGENCY_STOP(0)-급정지, SLOWDOWN_STOP(1)-감속정지
        public void SignalSeek(AXT_MOTION_HOME_DETECT detect, AXT_MOTION_EDGE edge, AXT_MOTION_STOPMODE mode)
        {
            lock (execLock)
            {
                Validate(CAXM.AxmMoveSignalSearch(Index,
                    LowVelocity, LowAcceleration, (int)detect, (int)edge, (int)mode));
            }
        }

        // 신호 검출 구동 - AxmMoveSignalSearch
        // velocity(검출이동속도)
        // acceleration(검출이동가감속도)
        // detect(검출신호)
        //		- PosEndLimit(0) - (+)Elm(End limit) +방향 리미트 센서 신호
        //		- NegEndLimit(1) - (-)Elm(End limit) -방향 리미트 센서 신호
        //		- PosSloLimit(2) - (+)Slm(Slow Down limit) 신호 - 사용하지 않음
        //		- NegSloLimit(3) - (-)Slm(Slow Down limit) 신호 - 사용하지 않음
        //		- HomeSensor(4)  - IN0(ORG) 원점 센서 신호
        //		- EncodZPhase(5) - IN1(Z상) Encoder Z상 신호
        //		- UniInput02(6)  - IN2(범용) 범용 입력 2번 신호
        //		- UniInput03(7)  - IN3(범용) 범용 입력 3번 신호
        // edge(검출Edge) : SIGNAL_DOWN_EDGE(0)-다운에지, SIGNAL_UP_EDGE(1)-업에지
        // mode(정지방법)  : EMERGENCY_STOP(0)-급정지, SLOWDOWN_STOP(1)-감속정지
        public void SignalSeek(double velocity, double acceleration, 
            AXT_MOTION_HOME_DETECT detect, AXT_MOTION_EDGE edge, AXT_MOTION_STOPMODE mode)
        {
            lock (execLock)
            {
                Validate(CAXM.AxmMoveSignalSearch(Index,
                    velocity, acceleration, (int)detect, (int)edge, (int)mode));
            }
        }

        // 구동 시작 - AxmMoveVel
        public void MoveStart()
        {
            lock (execLock)
            {
                Validate(CAXM.AxmMoveVel(Index, HiVelocity, HiAcceleration, HiDeceleration));
            }
        }

        // 구동 시작 - AxmMoveVel
        // velocity     - 구동 속도
        // acceleration - 가속 속도
        // deceleration - 감속 속도
        public void MoveStart(double velocity, double acceleration, double deceleration)
        {
            lock (execLock)
            {
                Validate(CAXM.AxmMoveVel(Index, velocity, acceleration, deceleration));
            }
        }

        // 구동 감속 정지 - AxmMoveStop
        public void MoveStop()
        {
            lock (execLock)
            {
                Validate(CAXM.AxmMoveStop(Index, HiDeceleration));
            }
        }

        // 구동 감속 정지 - AxmMoveStop
        // deceleration - 감속 속도
        public void MoveStop(double deceleration)
        {
            lock (execLock)
            {
                Validate(CAXM.AxmMoveStop(Index, deceleration));
            }
        }

        // 구동 급정지 - AxmMoveEStop
        public void MoveEStop()
        {
            lock (execLock)
            {
                Validate(CAXM.AxmMoveEStop(Index));
            }
        }

        // 지정거리이동 - AxmMoveStartPos
        // distance - 이동 거리
        public void MoveTo(double distance)
        {
            lock (execLock)
            {
                Validate(CAXM.AxmMoveStartPos(Index, distance, HiVelocity, HiAcceleration, HiDeceleration));
            }
        }

        // 지정거리이동 - AxmMoveStartPos
        // distance     - 이동 거리
        // velocity     - 이동 속도
        // acceleration - 가속 속도
        // deceleration - 감속 속도
        public void MoveTo(double distance, double velocity, double acceleration, double deceleration)
        {
            lock (execLock)
            {
                Validate(CAXM.AxmMoveStartPos(Index, distance, velocity, acceleration, deceleration));
            }
        }

        // 서보 상태 판정 - AxmStatusReadMotion
        // QIDRIVE_STATUS_0(0x0000001) - BUSY(드라이브 구동 중)
        // QIDRIVE_STATUS_1(0x0000002) - DOWN(감속 중)
        // QIDRIVE_STATUS_2(0x0000004) - CONST(등속 중)
        // QIDRIVE_STATUS_3(0x0000008) - UP(가속 중)
        // QIDRIVE_STATUS_4(0x0000010) - 연속 드라이브 구동 중
        // QIDRIVE_STATUS_5(0x0000020) - 지정 거리 드라이브 구동 중
        // QIDRIVE_STATUS_6(0x0000040) - MPG 드라이브 구동 중
        // QIDRIVE_STATUS_7(0x0000080) - 원점검색 드라이브 구동중
        // QIDRIVE_STATUS_8(0x0000100) - 신호 검색 드라이브 구동 중
        // QIDRIVE_STATUS_9(0x0000200) - 보간 드라이브 구동 중
        // QIDRIVE_STATUS_1(0x0000400) - 0 Slave 드라이브 구동중
        // QIDRIVE_STATUS_1(0x0000800) - 1 현재 구동 드라이브 방향(보간 드라이브에서는 표시 정보 다름)
        // QIDRIVE_STATUS_1(0x0001000) - 2 펄스 출력후 서보위치 완료 신호 대기중.
        // QIDRIVE_STATUS_1(0x0002000) - 3 직선 보간 드라이브 구동중.
        // QIDRIVE_STATUS_1(0x0004000) - 4 원호 보간 드라이브 구동중.
        // QIDRIVE_STATUS_1(0x0008000) - 5 펄스 출력 중.
        // QIDRIVE_STATUS_16(0x0010000) - 구동 예약 데이터 개수(처음)(0-7)
        // QIDRIVE_STATUS_17(0x0020000) - 구동 예약 데이터 개수(중간)(0-7)
        // QIDRIVE_STATUS_18(0x0040000) - 구동 예약 데이터 갯수(끝)(0-7)
        // QIDRIVE_STATUS_19(0x0080000) - 구동 예약 Queue 비어 있음.
        // QIDRIVE_STATUS_20(0x0100000) - 구동 예약 Queue 가득 ?
        // QIDRIVE_STATUS_21(0x0200000) - 현재 구동 드라이브의 속도 모드(처음)
        // QIDRIVE_STATUS_22(0x0400000) - 현재 구동 드라이브의 속도 모드(끝)
        // QIDRIVE_STATUS_23(0x0800000) - MPG 버퍼 #1 Full
        // QIDRIVE_STATUS_24(0x1000000) - MPG 버퍼 #2 Full
        // QIDRIVE_STATUS_25(0x2000000) - MPG 버퍼 #3 Full
        // QIDRIVE_STATUS_26(0x4000000) - MPG 버퍼 데이터 OverFlow
        public bool IsMotionState(AXT_MOTION_QIDRIVE_STATUS state)
        {
            return ((MotionState & (UInt32)state) == 0) ? false : true;
        }

        // 외부센서 및 모터관련신호 판정 - AxmStatusReadMechanical
        // QIMECHANICAL_PELM_LEVEL  (0x00001) - +Limit 급정지 신호 현재 상태
        // QIMECHANICAL_NELM_LEVEL  (0x00002) - -Limit 급정지 신호 현재 상태
        // QIMECHANICAL_PSLM_LEVEL  (0x00004) - +limit 감속정지 현재 상태.
        // QIMECHANICAL_NSLM_LEVEL  (0x00008) - -limit 감속정지 현재 상태
        // QIMECHANICAL_ALARM_LEVEL (0x00010) - Alarm 신호 신호 현재 상태
        // QIMECHANICAL_INP_LEVEL   (0x00020) - Inposition 신호 현재 상태
        // QIMECHANICAL_ESTOP_LEVEL (0x00040) - 비상 정지 신호(ESTOP) 현재 상태.
        // QIMECHANICAL_ORG_LEVEL   (0x00080) - 원점 신호 헌재 상태
        // QIMECHANICAL_ZPHASE_LEVEL(0x00100) - Z 상 입력 신호 현재 상태
        // QIMECHANICAL_ECUP_LEVEL  (0x00200) - ECUP 터미널 신호 상태.
        // QIMECHANICAL_ECDN_LEVEL  (0x00400) - ECDN 터미널 신호 상태.
        // QIMECHANICAL_EXPP_LEVEL  (0x00800) - EXPP 터미널 신호 상태
        // QIMECHANICAL_EXMP_LEVEL  (0x01000) - EXMP 터미널 신호 상태
        // QIMECHANICAL_SQSTR1_LEVEL(0x02000) - SQSTR1 터미널 신호 상태
        // QIMECHANICAL_SQSTR2_LEVEL(0x04000) - SQSTR2 터미널 신호 상태
        // QIMECHANICAL_SQSTP1_LEVEL(0x08000) - SQSTP1 터미널 신호 상태
        // QIMECHANICAL_SQSTP2_LEVEL(0x10000) - SQSTP2 터미널 신호 상태
        // QIMECHANICAL_MODE_LEVEL  (0x20000) - MODE 터미널 신호 상태.
        public bool IsSignalState(AXT_MOTION_QIMECHANICAL_SIGNAL state)
        {
            return ((SignalState & (UInt32)state) == 0) ? false : true;
        }

        private void Validate(UInt32 code)
        {
            base.Validate((AXT_FUNC_RESULT)code, "AxtAxisModule");
        }
    }

    public class AxtAxis : AxtObject
    {
        public AxtAxis()
        {
            axes = new List<AxtAxisModule>();
        }

        private List<AxtAxisModule> axes;

        public AxtAxisModule this[int index]
        {
            get
            {
                CheckRange(index);

                return axes[index];
            }
        }

        public AxtAxisModule this[string name]
        { get { return axes[GetIndex(name)]; } }

        public bool Active
        {
            get
            {
                foreach (AxtAxisModule axis in axes)
                {
                    if (axis.Active == false) return false;
                }

                return true;
            }

            set
            {
                foreach (AxtAxisModule axis in axes)
                {
                    axis.Active = value;
                }
            }
        }

        public bool Alarm
        {
            get
            {
                foreach (AxtAxisModule axis in axes)
                {
                    if (axis.Alarm == true) return true;
                }

                return false;
            }
        }

        public void Reset()
        {
            foreach (AxtAxisModule axis in axes)
            {
                axis.Reset = true;
            }

            Thread.Sleep(10);

            foreach (AxtAxisModule axis in axes)
            {
                axis.Reset = false;
            }
        }

        public int Count
        { get { return axes.Count; } }

        private int GetIndex(string name)
        {
            int index = -1;

            foreach (AxtAxisModule axis in axes)
            {
                if (axis.Name == name)
                {
                    index = axis.Index;
                    break;
                }
            }
            CheckRange(index);

            return index;
        }

        public void AddModule(AxtAxisModule axis)
        {
            axes.Add(axis);
        }

        private void CheckRange(int index)
        {
            if (index < 0)
            {
                throw new AxtException("Occurred invalid tag name Exception in AxtAxis");
            }
            else if (index >= axes.Count)
            {
                throw new AxtException("Occurred over range Exception in AxtAxis");
            }
        }
    }
}
