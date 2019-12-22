/****************************************************************************
*****************************************************************************
**
** File Name
** ---------
**
** AXS.CS
**
** COPYRIGHT (c) AJINEXTEK Co., LTD
**
*****************************************************************************
*****************************************************************************
**
** Description
** -----------
** Ajinextek Motion Library Header File
** 
**
*****************************************************************************
*****************************************************************************
**
** Source Change IndicesAxmStatusGetCmdPos
** ---------------------
**
** (None)
**
*****************************************************************************
*****************************************************************************
**
** Website
** ---------------------
**
** http://www.ajinextek.com
**
*****************************************************************************
*****************************************************************************
*/
using System;
using System.Runtime.InteropServices;

public class CAXS
{
    //========== 보드 및 모듈 확인함수(Info) - Infomation ===============================================================
    // 해당 축의 보드번호, 모듈 위치, 모듈 아이디를 반환한다.
    [DllImport("AXL.dll")] public static extern uint AxsInfoGetPort(int nAxisNo, ref int npBoardNo, ref int npModulePos, ref uint upModuleID);
	//지정한 모듈 번호로 해당 모듈의 Sub ID, 모듈 Name, 모듈 설명을 확인한다.
    //======================================================/
    // 지원 제품            : EtherCAT
    // upModuleSubID        : EtherCAT 모듈을 구분하기 위한 SubID
    // szModuleName         : 모듈의 모델명(up to 50 Byte)
    // szModuleDescription  : 모듈에 대한 설명(up to 80 Byte)
    //======================================================//    
    [DllImport("AXL.dll")] public static extern uint AxsInfoGetPortEx(int nPortNo, ref uint upModuleSubID, System.Text.StringBuilder szModuleName, System.Text.StringBuilder szModuleDescription);

    // 모션 모듈이 존재하는지 반환한다.
    [DllImport("AXL.dll")] public static extern uint AxsInfoIsSerialModule(ref uint upStatus);
    // 해당 포트가 유효한지 반환한다.
    [DllImport("AXL.dll")] public static extern uint AxsInfoIsInvalidPortNo(int nPortNo);
    // 해당 포트가 제어가 가능한 상태인지 반환한다.
    [DllImport("AXL.dll")] public static extern uint AxsInfoGetPortStatus(int nPortNo);
    // 시스템내 유효한 통신포트수를 반환한다.
    [DllImport("AXL.dll")] public static extern uint AxsInfoGetPortCount(ref int npPortCount);
    // 해당 보드/모듈의 첫번째 축번호를 반환한다.
    [DllImport("AXL.dll")] public static extern uint AxsInfoGetFirstPortNo(int nBoardNo, int nModulePos, ref int npPortNo);
    // 해당 보드의 첫번째 통신포트번호를 반환한다.
    [DllImport("AXL.dll")] public static extern uint AxsInfoGetBoardFirstPortNo(int nBoardNo, int nModulePos, ref int lpPortNo);
    //========== 시리얼 통신함수(Port) =================================================================================
    // 통신포트를 Open한다. PortOpen은 하나의 응용프로그램에서만 할 수 있음.
    // lBaudRate : 300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200
    // lDataBits : 7, 8 
    // lStopBits : 1, 2
    // lParity   : [0]None, [1]Even, [2]Odd
    // dwFlagsAndAttributes : Reserved
    [DllImport("AXL.dll")] public static extern uint AxsPortOpen(int nPortNo, int nBaudRate, int nDataBits, int nStopBits, int nParity, uint dwFlagsAndAttributes);
    // 통신포트를 Close한다.
    [DllImport("AXL.dll")] public static extern uint AxsPortClose(int nPortNo);
     // 통신포트를 설정한다.(통신 버퍼는 초기화되지 않음)
    // lpDCB->BaudRate  : 300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200
    // lpDCB->ByteSize  : 7, 8  
    // lpDCB->StopBits  : 1, 2
    // lpDCB->Parity    : [0]None, [1]Even, [2]Odd    
    [DllImport("kernel32.dll")] public static extern uint AxsPortSetCommState(int nPortNo, ref DCB lpDCB);
    // 통신포트의 설정값을 확인한다.
    [DllImport("kernel32.dll")] public static extern uint AxsPortGetCommState(int nPortNo, ref DCB lpDCB);
    // 통신포트의 타임아웃값을 설정한다.
    // lpCommTimeouts->ReadIntervalTimeout          : 문자열 입력이 시작된 후 문자열간 Timeout 시간 설정(milliseconds)
    // lpCommTimeouts->ReadTotalTimeoutMultiplier;  : 읽기 동작시 설정한 통신속도에서 하나의 문자열에 대한 Timeout시간 설정(milliseconds)
    // lpCommTimeouts->ReadTotalTimeoutConstant;    : 입력받을 문자수에 대한 Timeout을 제외한 Timeout시간 설정(milliseconds)
    // lpCommTimeouts->WriteTotalTimeoutMultiplier; : 쓰기 동작시 설정한 통신속도에서 하나의 문자열에 대한 Timeout시간 설정(milliseconds)
    // lpCommTimeouts->WriteTotalTimeoutConstant;   : 전송할 문자수에 대한 Timeout을 제외한 Timeout시간 설정(milliseconds)
    [DllImport("kernel32.dll")] public static extern uint AxsPortSetCommTimeouts(int nPortNo, ref COMMTIMEOUTS lpCommTimeouts);
    // 통신포트의 타임아웃값을 확인한다.
    [DllImport("kernel32.dll")] public static extern uint AxsPortGetCommTimeouts(int nPortNo, ref COMMTIMEOUTS lpCommTimeouts);
    // 장치의 오류 Flag를 지우거나 송수신된 데이타의 갯수를 확인한다.
    // lpErrors : 
    //      [1]CE_RXOVER:       수신버퍼 Overflow 발생
    //      [2]CE_OVERRUN:      수신버퍼 Overrun 에러발생
    //      [4]CE_RXPARITY:     수신 데이타 패리티비트 에러발생
    //      [8]CE_FRAME:        수신 Framing 에러발생      
    // lpStat->cbInQue :        수신버퍼에 입력된 데이타 갯수
    // lpStat->cbOutQue:        송신버퍼에 남은 데이타 갯수
    [DllImport("kernel32.dll")] public static extern uint AxsPortClearCommError(int nPortNo, out uint lpErrors, ref COMSTAT lpStat);
    // 데이타 송신을 멈춤
    [DllImport("AXL.dll")] public static extern uint AxsPortSetCommBreak(int nPortNo);
    // 데이타 송신을 재개
    [DllImport("AXL.dll")] public static extern uint AxsPortClearCommBreak(int nPortNo);	    
    // 송수신을 정지하거나 버퍼를 지움
    // dwFlags: 
    //      [1]PURGE_TXABORT:    쓰기 작업을 정지함
    //      [2]PURGE_RXABORT:    읽기 작업을 정지함
    //      [4]PURGE_TXCLEAR:    송신 버퍼에 데이타가 있을경우 지움
    //      [8]PURGE_RXCLEAR:    수신 버퍼에 데이타가 있을경우 지움
    [DllImport("AXL.dll")] public static extern uint AxsPortPurgeComm(int nPortNo, uint dwFlags);
    // 시리얼 포트에 데이타를 씀
    // lpBuffer :                장치에 쓸 데이터를 담는 버퍼의 포인트값
    // nNumberOfBytesToWrite :   lpBuffer에 담긴 실제 데이터의 바이트 수
    // lpNumberOfBytesWritten :  실제로 쓰여진 바이트 수를 반환(None Overrapped 일경우)
    // lpOverlapped :            비동기를 위한 OVERLAPPED 구조체의 포인트 값
    [DllImport("kernel32.dll")] public static extern uint AxsPortWriteFile(int nPortNo, IntPtr lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, ref OVERLAPPED lpOverlapped);
    // 시리얼 포트의 데이타를 읽음
    // lpBuffer :               장치에 쓸 데이터를 담는 버퍼의 포인트값
    // nNumberOfBytesToRead :   lpBuffer가 가리키는 버퍼의 크기를 바이트로 지정
    // lpNumberOfBytesRead :    실제로 읽혀진 바이트 수를 반환(None Overrapped 일경우)
    // lpOverlapped :           비동기를 위한 OVERLAPPED 구조체의 포인트 값
    [DllImport("kernel32.dll")] public static extern uint AxsPortReadFile(int nPortNo, IntPtr lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, ref OVERLAPPED lpOverlapped);

    // 시리얼 포트의 Overlapped 작업의 결과에 대해 반환 
    // lpOverlapped->hEvent :       전송이 완료된 후 시그널될 이벤트 핸들. AxsPortWriteFile, AxsPortReadFile 함수를 사용하기전에 이 값을 설정함.
    // lpNumberOfBytesTransferred:  실제 전송이 완료된 바이트 크기를 얻기 위한 변수포인트
    // bWait:                       I/O 연산이 끝나지않은 상황에서의 처리를 결정
    //      [0]: I/O연산이 끝날때까지 기다림
    //      [1]: I/O연산이 끝나지 않아도 반환함
    [DllImport("AXL.dll")] public static extern uint AxsPortGetOverlappedResult(int nPortNo, ref OVERLAPPED lpOverlapped, out uint lpNumberOfBytesTransferred, bool bWait);
    
    // 시리얼 포트에 발생한 최종 에러코드를 반환    
    // [  0]ERROR_SUCCESS          에러없음
    // [  5]ERROR_ACCESS_DENIED    통신포트가 사용중일 경우
    // [  6]ERROR_INVALID_HANDLE   통신포트가 유효하지 않을 경우(네트워크 연결오류포함)
    // [ 87]ERROR_INVALID_PARAMETER 잘못된 파라메타 설정   
    // [995]ERROR_OPERATION_ABORTED The I/O operation has been aborted because of either a thread exit or an application request.
    // [996]ERROR_IO_INCOMPLETE     Overrapped모드일 때 쓰기작업이 끝나지 않은경우나 Timeout이 발생한 경우
    // [997]ERROR_IO_PENDING        Overrapped모드일 때 I/O작업이 진행중인 경우
    // [998]ERROR_NOACCESS
    [DllImport("AXL.dll")] public static extern uint AxsPortGetLastError(int nPortNo, ref uint dwpErrCode);
    [DllImport("AXL.dll")] public static extern uint AxsPortSetLastError(int nPortNo, uint dwErrCode);
    
}
