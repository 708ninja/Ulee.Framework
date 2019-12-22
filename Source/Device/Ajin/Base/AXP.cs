/****************************************************************************
*****************************************************************************
**
** File Name
** ---------
**
** AXP.CS
**
** COPYRIGHT (c) AJINEXTEK Co., LTD
**
*****************************************************************************
*****************************************************************************
**
** Description
** -----------
** Ajinextek Macro Library Header File
** 
**
*****************************************************************************
*****************************************************************************
**
** Source Change Indices
** ---------------------
**
** (None)
**
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

using System.Runtime.InteropServices;

public class CAXP
{
    // 지정된 매크로에 노드 등록을 시작한다.
    // lMacroNo     : 0 ~ 7
    [DllImport("AXL.dll")] public static extern uint AxpMacroBeginNode(int nMacroNo);    
    // 지정된 매크로에 노드 등록을 종료한다.
    // lMacroNo     : 0 ~ 7
    [DllImport("AXL.dll")] public static extern uint AxpMacroEndNode(int nMacroNo);    
    // 지정된 매크로에 등록된 노드들을 모두 삭제한다. 
    // lMacroNo     : 등록된 노드들을 모두 삭제 할 매크로 번호를 지정
    //  [-1]        : -1일 경우 모든 매크로에 등록된 노드들을 모두 삭제한다.
    //  [0 ~ 7]     : 지정한 매크로에 등록된 노드들을 모두 삭제한다.
    [DllImport("AXL.dll")] public static extern uint AxpMacroWriteClear(int nMacroNo);
    
    // 지정된 매크로에 노드를 등록한다.
    // lMacroNo     : 0 ~ 7
    // uFunction    : 매크로에 등록한 함수를 지정한다.{ MACRO_FUNCTION }
    //  [0] MACRO_FUNC_CALL         : 다른 매크로를 호출한다.(호출된 매크로가 종료되거나 RETURN하면 다시 돌아옴)
    //  [1] MACRO_FUNC_JUMP         : 다른 매크로나 노드로 점프한다. (JUMP위치로 되돌아오지 않음)
    //  [2] MACRO_FUNC_RETURN       : 호출한 매크로로 되돌아감
    //  [3] MACRO_FUNC_REPEAT       : 현재 위치에서 지정한 노드사이를 설정한 횟수만큼 반복구동함
    //  [4] MACRO_FUNC_SET_OUTPUT   : Digital Output, Analog Output을 출력함
    //  [5] MACRO_FUNC_WAIT         : 지정한 시간만큼 대기함
    //  [6] MACRO_FUNC_STOP         : 매크로 구동을 정지함
    // dpArg[16] : 지정한 Function에 대한 인자 배열
    // ※ 주의점 : 배열의 개수는 반드시 16개로 해야 됨
    //  [0] MACRO_FUNC_CALL
    //      dArg[0] Macro 번호  
    //  [1] MACRO_FUNC_JUMP
    //      dArg[0] Jump Type       : [0] MACRO_JUMP_MACRO, [1] MACRO_JUMP_NODE
    //      dArg[1] 매크로번호 or 노드번호 
    //  [2] MACRO_FUNC_RETURN       : 인자 없음
    //  [3] MACRO_FUNC_REPEAT       : 반복구동 지정
    //      dArg[0] 반복할 회수
    //      dArg[1] 현재 위치에서 반복할 노드 번호(현재 노드의 번호보다 낮은 번호여야 됨)
    //  [4] MACRO_FUNC_SET_OUTPUT
    //      dArg[0] Output Type     : [0] MACRO_DIGITAL_OUTPUT, [1] MACRO_ANALOG_OUTPUT
    //      dArg[1] DO 모듈번호 or AO 채널번호
    //      dArg[2] Data Type
    //        - Digital Output일경우: [0] MACRO_DATA_BIT, [1] MACRO_DATA_BYTE, [2] MACRO_DATA_WORD, [3] MACRO_DATA_DWORD, [4] MACRO_DATA_BYTE12 
    //        - Analog Output일경우 : [5] MACRO_DATA_VOLTAGE, [6] MACRO_DATA_DIGIT
    //      dArg[3] Offset or Byte Size
    //        - Digital Output일경우: Offset / ByteSize(MACRO_DATA_BYTE12 Type일 경우)
    //        - Analog Output일경우 : Reserved
    //      dArg[4] 출력 값
    //        - Digital Output일경우: Data Type의 설정에 따른 Digital Output, 
    //        - Analog Output일경우 : 출력전압 or 출력 Digit
    //      dArg[4] ~ dArg[15] Byte 출력값 (MACRO_DATA_BYTE12 Type일 경우)
    //        - 지정한 Byte(최대 12Byte)값들을 여러개의 출력 모듈에 상관없이 한번에 출력함
    //  [5] MACRO_FUNC_WAIT
    //      dArg[0] 대기 시간(ms)   : 지정한 ms 동안 대기함
    //  [6] MACRO_FUNC_STOP
    //      dArg[0] 정지모드        : [0] MACRO_QUICK_STOP, [1] MACRO_SLOW_STOP
    [DllImport("AXL.dll")] public static extern uint AxpMacroAddNode(int nMacroNo, uint uFunction, ref double dpArg);
    // 지정된 매크로의 노드에 설정된 값을 반환한다. (반환값은 AxpMacroAddNode 함수로 설정한 값임)
    [DllImport("AXL.dll")] public static extern uint AxpMacroGetNode(int nMacroNo, int nNodeNo, ref uint upFunction, ref double dpArg);
    // 지정된 매크로에 등록된 노드들을 검사한 후 에러가 발생 노드 위치와 에러 코드를 반환한다.
    // ※ 주의점 : 반드시 이 함수로 등록된 노들들에 대해 유효성 검사를 완료해야 매크로 구동을 할 수 있음.
    // lMacroNo         : 0 ~ 7
    // *lpErrorNodeNo   : 에러가 발생한 노드번호
    // *upErrorCode     : 에러가 발생한 노드에 대한 에러코드
    [DllImport("AXL.dll")] public static extern uint AxpMacroCheckNodeAll(int nMacroNo, ref int npErrorNodeNo, ref uint upErrorCode);
    // 지정된 매크로의 구동을 시작한다.
    // ※ 주의점  : 반드시 AxpMacroCheckNodeAll 함수로 등록된 노들들에 대해 유효성 검사를 완료해야 매크로 구동을 할 수 있음.
    // lMacroNo         : 0 ~ 7
    // uCondition       : 매크로 시작 조건을 설정함
    //  [0] MACRO_START_READY       : 대기 상태로 구동
    //  [1] MACRO_START_IMMEDIATE   : 즉시 구동
    // bLockResource    : 매크로에 등록된 디지털 입력모듈, 아날로그 출력 채널들에 대해 제어금지 여부를 설정
    //  [FALSE]         : 제어허용
    //  [TRUE]          : 제어금지
    // lRepeatCount     : 매크로 전체 구동의 반복 회수를 설정
    //  [-1]            : 무한반복 구동함
    //  [0]             : 반복구동하지 않음
    //  [1~ ]           : 지정한 회수만큼 반복 구동함
    [DllImport("AXL.dll")] public static extern uint AxpMacroStart(int nMacroNo, uint uCondition, bool bLockResource, int nRepeatCount);
    // 지정된 매크로의 구동 조건을 반환한다.
    // lMacroNo         : 0 ~ 7
    [DllImport("AXL.dll")] public static extern uint AxpMacroGetStartInfo(int nMacroNo, ref uint upCondition, ref bool bpLockResource, ref int npRepeatCount);
    // 지정된 매크로의 구동을 정지한다.
    // lMacroNo         : -1 ~ 7
    //  [-1]            : -1일 경우 모든 매크로의 구동을 정지한다.
    // uStopMode        : 구동정지 모드를 지정한다. (등록된 매크로에 모션 구동축이 있을 경우 의미가 있음)
    //  [0] MACRO_QUICK_STOP
    //  [1] MACRO_SLOW_STOP
    [DllImport("AXL.dll")] public static extern uint AxpMacroStop(int nMacroNo, uint uStopMode);
    // 지정된 Macro의 구동상태를 반환한다.
    // lMacroNo         : 0 ~ 7
    // *upStatus        : 매크로의 구동상태
    //  [0] MACRO_STATUS_STOP   : 매크로 정지상태
    //  [1] MACRO_STATUS_READY  : 매크로 구동 대기 상태
    //  [2] MACRO_STATUS_RUN    : 매크로 구동상태
    //  [3] MACRO_STATUS_ERROR  : 매크로 에러상태(정지상태)
    // *lpRepeatCount   : 매크로의 전체 반복구동 회수를 반환
    [DllImport("AXL.dll")] public static extern uint AxpMacroReadRunStatus(int nMacroNo, ref uint upStatus, ref int npRepeatCount);
    // 지정된 매크로에 등록된 노드개수와 현재 구동중인 노드정보를 반환한다.
    // lMacroNo         : 0 ~ 7
    // *lpCurNodeNo     : 현재 구동중인 노드의 번호
    // *lpTotalNodeNum  : 지정한 매크로의 전체 노드 개수
    [DllImport("AXL.dll")] public static extern uint AxpMacroGetNodeNum(int nMacroNo, ref int npCurNodeNo, ref int npTotalNodeNum);
    // 지정된 매크로와 함수에 대한 구동정보를 반환한다.
    // lMacroNo         : 0 ~ 7
    // uFunction        : 정보를 확인할 함수를 지정한다.{ MACRO_FUNCTION }
    //  [0] MACRO_FUNC_CALL
    //  [1] MACRO_FUNC_JUMP
    //  [2] MACRO_FUNC_RETURN
    //  [3] MACRO_FUNC_REPEAT
    //  [4] MACRO_FUNC_SET_OUTPUT
    //  [5] MACRO_FUNC_WAIT
    //  [6] MACRO_FUNC_STOP
    // dpReturnData[16]  : 지정한 함수에 대한 반환값
    // ※ 주의점 : 배열의 개수는 반드시 16개로 해야 됨
    //  [0] MACRO_FUNC_CALL
    //      dArg[0] 최종 호출 된 CALL명령의 노드번호
    //      dArg[1] 지정한 매크로 내에서 CALL명령들 중의 Index를 반환
    //  [1] MACRO_FUNC_JUMP
    //      dArg[0] 최종 호출된 JUMP명령의 노드번호
    //      dArg[1] 지정한 매크로 내에서 JUMP명령들 중의 Index를 반환
    //  [2] MACRO_FUNC_RETURN       : 인자 없음
    //      dArg[0] 최종 호출된 RETURN명령의 노드번호
    //      dArg[1] 지정한 매크로 내에서 RETURN명령들 중의 Index를 반환
    //  [3] MACRO_FUNC_REPEAT       : 반복구동 지정
    //      dArg[0] 최종 호출된 REPEAT명령의 노드번호
    //      dArg[1] 지정한 매크로 내에서 REPEAT명령들 중의 Index를 반환
    //      dArg[2] 최종 호출된 REPEAT명령의 반복구동 설정회수를 반환
    //      dArg[3] 최종 호출된 REPEAT명령의 반복구동한 회수를 반환
    //  [4] MACRO_FUNC_SET_OUTPUT
    //      dArg[0] 최종 호출된 SET_OUTPUT명령의 노드번호
    //      dArg[1] 지정한 매크로 내에서 SET_OUTPUT명령들 중의 Index를 반환
    //      dArg[2] 최종 호출된 SET_OUTPUT명령의 출력값을 반환
    //      dArg[2~13] Data Type이 "[4] MACRO_DATA_BYTE12"일 경우의 Byte 출력값
    //  [5] MACRO_FUNC_WAIT
    //      dArg[0] 최종 호출된 WAIT명령의 노드번호
    //      dArg[1] 지정한 매크로 내에서 WAIT명령들 중의 Index를 반환
    //      dArg[2] 최종 호출된 WAIT명령의 설정 시간을 반환
    //      dArg[3] 최종 호출된 WAIT명령의 소요 시간을 반환
    //  [6] MACRO_FUNC_STOP
    //      dArg[0] 최종 호출된 STOP명령의 노드번호
    //      dArg[1] 지정한 매크로 내에서 STOP명령들 중의 Index를 반환
    [DllImport("AXL.dll")] public static extern uint AxpMacroReadFunctionStatus(int nMacroNo, uint uFunction, ref double dpReturnData);
}