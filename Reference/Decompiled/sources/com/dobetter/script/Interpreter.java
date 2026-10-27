package com.dobetter.script;

import com.dobetter.common.GZIP;
import com.dobetter.common.Tool;
import java.io.ByteArrayInputStream;
import java.io.DataInputStream;
import java.io.EOFException;
import java.io.IOException;
import java.io.InputStream;
import java.util.Enumeration;
import java.util.Hashtable;
import java.util.Vector;

/* JADX INFO: loaded from: classes.dex */
public class Interpreter {
    public static final int ERR_ARR_OUT = 9;
    public static final int ERR_ILLEGAL = 0;
    public static final int ERR_NOSUB = 3;
    public static final int ERR_NO_SRC = 7;
    public static final int ERR_NO_VAR = 4;
    public static final int ERR_OPSYMB = 8;
    public static final int ERR_PAESEPARA = 6;
    public static final int ERR_PARA_CALC = 5;
    public static final int ERR_TYPE_INVALID = 2;
    public static final int ERR_VAR = 1;
    public static final String FUNC_FLAG = "x";
    private static final int KEYWORD_ELSE = 1;
    private static final int KEYWORD_ENDIF = 2;
    private static final int KEYWORD_IF = 0;
    private static final int KEYWORD_LOOP = 4;
    private static final int KEYWORD_RET = 6;
    private static final int KEYWORD_SUB = 5;
    private static final int KEYWORD_WHILE = 3;
    private static final String STR_NUMERIC = "0123456789";
    private static final String STR_SYMBOL = "+-*/><()=, []:&|!'";
    private boolean bFinished;
    private boolean bPause;
    private boolean bStarted;
    private Hashtable golbleVar;
    private int instrucPointer;
    private boolean isTopCall;
    public ScriptProcessor scriptProcessor;
    private String[] srcCode;
    private static final String[] STRS_RESERVED = {"if", "else", "endif", "while", "loop", "sub", "ret"};
    public static final String[] STRS_ERR = {" Illegal statment ,", " Invalid variable name ", " Data type error ", " No such method ", " No such variable ", " Method parameter error ", " Parameter count error ", " Code not load yet ", " Operation symbol error  ", " Array out of bounds  "};
    public static Hashtable globalValueHT = new Hashtable();

    public Interpreter(ScriptProcessor processor) {
        this.scriptProcessor = processor;
    }

    public void startScript(String file) {
        this.srcCode = null;
        load(file);
        this.instrucPointer = 0;
        this.scriptProcessor.setScriptState((byte) 1);
        this.bStarted = true;
        this.bPause = false;
        this.bFinished = false;
        startProcess(null);
    }

    public void pause() {
        this.scriptProcessor.setScriptState((byte) 2);
        this.bPause = true;
    }

    public void continueProcess() {
        if (this.scriptProcessor.getScriptState() == 3) {
            this.bPause = false;
            this.scriptProcessor.setScriptState((byte) 1);
            this.isTopCall = true;
            _sub(null, this.instrucPointer);
        }
    }

    public void finish() {
        this.bStarted = false;
        this.bFinished = true;
        this.scriptProcessor.setScriptState((byte) 4);
    }

    /* JADX WARN: Code duplicated, block: B:15:0x003f A[LOOP:1: B:14:0x003d->B:15:0x003f, LOOP_END] */
    private void load(String path) {
        int i;
        InputStream in = Tool.getResourceAsStream(path);
        byte[] data = (byte[]) null;
        try {
            byte[] data2 = Tool.getBytesFromInput(in);
            data = GZIP.inflate(data2);
        } catch (IOException e1) {
            e1.printStackTrace();
        }
        if (data != null) {
            ByteArrayInputStream bin = new ByteArrayInputStream(data);
            DataInputStream din = new DataInputStream(bin);
            Vector vCode = new Vector();
            int count = 0;
            while (true) {
                try {
                    String str = din.readUTF();
                    vCode.addElement(str);
                    count++;
                } catch (EOFException e) {
                    try {
                        din.close();
                        bin.close();
                    } catch (Exception e2) {
                        e2.printStackTrace();
                    }
                    this.srcCode = new String[count];
                    for (i = 0; i < count; i++) {
                        this.srcCode[i] = (String) vCode.elementAt(i);
                    }
                    return;
                } catch (IOException e3) {
                    e3.printStackTrace();
                    din.close();
                    bin.close();
                    this.srcCode = new String[count];
                    while (i < count) {
                        this.srcCode[i] = (String) vCode.elementAt(i);
                    }
                    return;
                }
            }
        }
    }

    public Object startProcess() {
        return startProcess(null);
    }

    public Object startProcess(Hashtable env) {
        this.isTopCall = true;
        this.golbleVar = new Hashtable();
        try {
            if (this.srcCode == null) {
                errout(0, STRS_ERR[7]);
                return null;
            }
            if (env != null) {
                Enumeration e = env.keys();
                while (e.hasMoreElements()) {
                    Object key = e.nextElement();
                    Object val = env.get(key);
                    this.golbleVar.put(((String) key).toLowerCase(), val);
                }
            }
            return callSub(null, null);
        } catch (Exception ex) {
            Tool.reportException(ex);
            return null;
        }
    }

    private Object _sub(Vector paraStack, int pointer) {
        Hashtable localVar;
        this.instrucPointer = pointer;
        if (this.isTopCall) {
            localVar = this.golbleVar;
            this.isTopCall = false;
        } else {
            localVar = new Hashtable();
        }
        while (this.instrucPointer < this.srcCode.length) {
            if (this.bFinished || this.bPause) {
                if (this.bFinished) {
                    finish();
                }
                return null;
            }
            try {
                String instruct = this.srcCode[this.instrucPointer];
                System.out.println(String.valueOf(this.instrucPointer) + " " + instruct);
                if (instruct.length() != 0) {
                    int keywordCode = getKeywordCode(getFirstWord(instruct));
                    if (keywordCode >= 0) {
                        switch (keywordCode) {
                            case 0:
                                Bool ib = (Bool) calcExpr(instruct.substring(STRS_RESERVED[0].length()), localVar);
                                if (!ib.getVal()) {
                                    int countIf = 1;
                                    for (int i = this.instrucPointer + 1; i < this.srcCode.length; i++) {
                                        String tmpHead = getFirstWord(this.srcCode[i]);
                                        if (tmpHead.equals(STRS_RESERVED[0])) {
                                            countIf++;
                                        } else if (tmpHead.equals(STRS_RESERVED[2])) {
                                            countIf--;
                                        }
                                        if (tmpHead.equals(STRS_RESERVED[1]) && countIf == 1) {
                                            this.instrucPointer = i;
                                        }
                                        if (tmpHead.equals(STRS_RESERVED[2]) && countIf == 0) {
                                            this.instrucPointer = i;
                                        }
                                        break;
                                        break;
                                    }
                                }
                                break;
                            case 1:
                                int countIf2 = 1;
                                for (int i2 = this.instrucPointer + 1; i2 < this.srcCode.length; i2++) {
                                    String tmpHead2 = getFirstWord(this.srcCode[i2]);
                                    if (tmpHead2.equals(STRS_RESERVED[0])) {
                                        countIf2++;
                                    } else if (tmpHead2.equals(STRS_RESERVED[2])) {
                                        countIf2--;
                                    }
                                    if (tmpHead2.equals(STRS_RESERVED[2]) && countIf2 == 0) {
                                        this.instrucPointer = i2;
                                    }
                                    break;
                                }
                                break;
                            case 3:
                                Bool wb = (Bool) calcExpr(instruct.substring(STRS_RESERVED[3].length()), localVar);
                                if (!wb.getVal()) {
                                    int countWhile = 1;
                                    for (int i3 = this.instrucPointer + 1; i3 < this.srcCode.length; i3++) {
                                        String tmpHead3 = getFirstWord(this.srcCode[i3]);
                                        if (tmpHead3.equals(STRS_RESERVED[3])) {
                                            countWhile++;
                                        } else if (tmpHead3.equals(STRS_RESERVED[4])) {
                                            countWhile--;
                                        }
                                        if (tmpHead3.equals(STRS_RESERVED[4]) && countWhile == 0) {
                                            this.instrucPointer = i3;
                                        }
                                        break;
                                    }
                                }
                                break;
                            case 4:
                                int countWhile2 = 1;
                                for (int i4 = this.instrucPointer - 1; i4 > 0; i4--) {
                                    String tmpHead4 = getFirstWord(this.srcCode[i4]);
                                    if (tmpHead4.equals(STRS_RESERVED[3])) {
                                        countWhile2--;
                                    } else if (tmpHead4.equals(STRS_RESERVED[4])) {
                                        countWhile2++;
                                    }
                                    if (tmpHead4.equals(STRS_RESERVED[3]) && countWhile2 == 0) {
                                        this.instrucPointer = i4 - 1;
                                    }
                                    break;
                                }
                                break;
                            case 5:
                                return null;
                            case 6:
                                finish();
                                if (instruct.length() > STRS_RESERVED[6].length()) {
                                    return calcExpr(instruct.substring(instruct.indexOf(" ")), localVar);
                                }
                                return null;
                        }
                    } else if (isArr(instruct)) {
                        _setArr(instruct, localVar);
                    } else if (isSubCall(instruct)) {
                        callSub(instruct, localVar);
                    } else if (isVar(instruct)) {
                        _setVar(instruct, localVar);
                    } else {
                        errout(this.instrucPointer, STRS_ERR[0]);
                    }
                }
                this.instrucPointer++;
            } catch (Exception ex) {
                errout(this.instrucPointer, String.valueOf(STRS_ERR[0]) + ex.getMessage());
                ex.printStackTrace();
            }
        }
        finish();
        return null;
    }

    private String getFirstWord(String instruct) {
        String instruct2 = instruct.trim();
        StringBuffer tsb = new StringBuffer();
        for (int i = 0; i < instruct2.length() && !isSymbol(instruct2.charAt(i)); i++) {
            tsb.append(instruct2.charAt(i));
        }
        return tsb.toString().trim().toLowerCase();
    }

    private int getKeywordCode(String s) {
        if (s == null || s.length() == 0) {
            return -1;
        }
        for (int i = 0; i < STRS_RESERVED.length; i++) {
            if (s.equals(STRS_RESERVED[i])) {
                return i;
            }
        }
        return -1;
    }

    private boolean isSubCall(String s) {
        return getInstructType(s) == '(';
    }

    private boolean isVar(String s) {
        return getInstructType(s) == '=';
    }

    private boolean isArr(String s) {
        return getInstructType(s) == '[';
    }

    private char getInstructType(String instruct) {
        for (int i = 0; i < instruct.length(); i++) {
            if ("(=[".indexOf(instruct.charAt(i)) >= 0) {
                return instruct.charAt(i);
            }
        }
        return (char) 0;
    }

    private boolean isSymbol(char ch) {
        return STR_SYMBOL.indexOf(ch) >= 0;
    }

    private boolean isNumeric(char ch) {
        return STR_NUMERIC.indexOf(ch) >= 0;
    }

    private boolean isLetter(char ch) {
        return (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');
    }

    private Vector parseInstruct(String s) {
        char ch;
        Vector stack = new Vector();
        int len = s.length();
        StringBuffer sb = new StringBuffer();
        int i = 0;
        while (i < s.length()) {
            char ch2 = s.charAt(i);
            if (ch2 != ' ') {
                if (ch2 == '/') {
                    break;
                }
                if (ch2 == '\"') {
                    sb.append(ch2);
                    do {
                        i++;
                        if (i >= s.length()) {
                            break;
                        }
                        ch = s.charAt(i);
                        sb.append(ch);
                    } while (ch != '\"');
                } else if (isSymbol(ch2)) {
                    sb.append(ch2);
                    if (ch2 == '>') {
                        if (i + 1 < len && s.charAt(i + 1) == '=') {
                            sb.append(s.charAt(i + 1));
                            i++;
                        }
                    } else if (ch2 == '<' && i + 1 < len && (s.charAt(i + 1) == '=' || s.charAt(i + 1) == '>')) {
                        sb.append(s.charAt(i + 1));
                        i++;
                    }
                } else if (isLetter(ch2)) {
                    while (i < s.length()) {
                        ch2 = s.charAt(i);
                        if (isSymbol(ch2)) {
                            break;
                        }
                        sb.append(ch2);
                        i++;
                    }
                    while (i < s.length() && (ch2 = s.charAt(i)) == ' ') {
                        i++;
                    }
                    if (ch2 == '[') {
                        int leftQ = 0;
                        while (true) {
                            if (ch2 == '[') {
                                leftQ++;
                            }
                            if (ch2 == ']') {
                                leftQ--;
                            }
                            sb.append(ch2);
                            i++;
                            if (i >= s.length()) {
                                break;
                            }
                            ch2 = s.charAt(i);
                            if ("+-*/,)=:><".indexOf(ch2) >= 0 && leftQ <= 0) {
                                break;
                            }
                        }
                    } else if (ch2 == '(') {
                        int leftQ2 = 0;
                        do {
                            char ch3 = s.charAt(i);
                            if (ch3 == '(') {
                                leftQ2++;
                            }
                            if (ch3 == ')') {
                                leftQ2--;
                            }
                            sb.append(ch3);
                            i++;
                            if (leftQ2 <= 0) {
                                break;
                            }
                        } while (i < s.length());
                    }
                    i--;
                } else if (isNumeric(ch2)) {
                    while (i < s.length()) {
                        char ch4 = s.charAt(i);
                        if (isSymbol(ch4)) {
                            i--;
                            break;
                        }
                        sb.append(ch4);
                        i++;
                    }
                }
                vPushBack(stack, sb.toString());
                sb.setLength(0);
            }
            i++;
        }
        return stack;
    }

    private void errout(int ip, String s) {
        System.out.println(String.valueOf(ip + 1) + " : " + s);
    }

    private boolean isStr(Object o) {
        return o instanceof Str;
    }

    private boolean isInt(Object o) {
        return o instanceof Int;
    }

    private boolean isArr(Object o) {
        return o instanceof Array;
    }

    private boolean isBool(Object o) {
        return o instanceof Bool;
    }

    private boolean isSymb(Object o) {
        return o instanceof Symb;
    }

    public static final Object vPopFront(Vector v) {
        if (v.size() <= 0) {
            return null;
        }
        Object objFirstElement = v.firstElement();
        if (v.size() > 0) {
            v.removeElementAt(0);
            return objFirstElement;
        }
        return objFirstElement;
    }

    public static final Object vPopBack(Vector v) {
        if (v.size() <= 0) {
            return null;
        }
        Object objLastElement = v.lastElement();
        if (v.size() > 0) {
            v.removeElementAt(v.size() - 1);
            return objLastElement;
        }
        return objLastElement;
    }

    public static final int vPopIntBack(Vector v) {
        Object ob = vPopBack(v);
        if (ob == null) {
            return 0;
        }
        return ((Int) ob).getVal();
    }

    public static final boolean vPopBooleanBack(Vector v) {
        Object ob = vPopBack(v);
        if (ob == null) {
            return false;
        }
        return ((Bool) ob).getVal();
    }

    public static final String vPopStringBack(Vector v) {
        Object ob = vPopBack(v);
        if (ob == null) {
            return null;
        }
        return ((Str) ob).getVal();
    }

    public static final void vPushFront(Vector v, Object o) {
        v.insertElementAt(o, 0);
    }

    public static final void vPushBack(Vector v, Object o) {
        v.addElement(o);
    }

    private Object _getVar(String name, Hashtable localVar) {
        String name2 = name.toLowerCase();
        Object value = localVar.get(name2);
        if (value == null) {
            value = this.golbleVar.get(name2);
        }
        if (value != null) {
            return value;
        }
        return null;
    }

    private void _setVar(String instruct, Hashtable varList) throws Exception {
        String exprStr = instruct.substring(instruct.indexOf(61) + 1);
        Vector expr = parseInstruct(instruct);
        String varName = ((String) vPopFront(expr)).toLowerCase();
        Object varValue = _getVar(varName, varList);
        String symb = (String) vPopFront(expr);
        if (symb.charAt(0) == '=') {
            Object newVarValue = calcExpr(exprStr, varList);
            if (newVarValue != null) {
                if (varValue == null) {
                    varList.put(varName, newVarValue);
                    return;
                }
                if ((isStr(newVarValue) && isStr(varValue)) || ((isInt(newVarValue) && isInt(varValue)) || ((isBool(newVarValue) && isBool(varValue)) || (isArr(newVarValue) && isArr(varValue))))) {
                    varList.put(varName, newVarValue);
                    return;
                }
                throw new Exception(STRS_ERR[2]);
            }
            return;
        }
        throw new Exception(STRS_ERR[1]);
    }

    /* JADX WARN: Code duplicated, block: B:116:0x01e2 A[LOOP:4: B:84:0x012d->B:116:0x01e2, LOOP_END] */
    /* JADX WARN: Code duplicated, block: B:132:0x022b A[LOOP:6: B:96:0x0169->B:132:0x022b, LOOP_END] */
    /* JADX WARN: Code duplicated, block: B:147:0x017b A[SYNTHETIC] */
    /* JADX WARN: Code duplicated, block: B:151:0x00f3 A[SYNTHETIC] */
    /* JADX WARN: Code duplicated, block: B:88:0x013f  */
    Object calcExpr(String exprStr, Hashtable varList) throws Exception {
        int cType1;
        Vector expr = preCalc(parseInstruct(exprStr), varList);
        int cType = 0;
        for (int i = 0; i < expr.size(); i++) {
            Object ob = expr.elementAt(i);
            if (isStr(ob)) {
                cType |= 2;
            } else if (isSymb(ob)) {
                if (((Symb) ob).isLogicOp()) {
                    cType |= 16;
                }
            } else if (isBool(ob)) {
                cType |= 4;
            } else if (isArr(ob)) {
                cType |= 8;
            } else if (isInt(ob)) {
                cType |= 1;
            }
        }
        if ((cType & 1) != 0 && (cType & 2) == 0 && (cType & 8) == 0 && (cType & 4) == 0 && (cType & 16) == 0) {
            cType1 = 1;
        } else if ((cType & 2) != 0 && (cType & 16) == 0) {
            cType1 = 2;
        } else if (((cType & 4) != 0 || (cType & 16) != 0) && (cType & 2) == 0 && (cType & 8) == 0) {
            cType1 = 4;
        } else if ((cType & 8) != 0 && (cType & 2) == 0 && (cType & 1) == 0 && (cType & 4) == 0 && (cType & 16) == 0) {
            cType1 = 8;
        } else {
            throw new Exception(STRS_ERR[0]);
        }
        switch (cType1) {
            case 1:
                break;
            case 2:
                calcExprStrImpl(expr);
                Object resultDt = vPopBack(expr);
                return resultDt;
            case 3:
            case 5:
            case 6:
            case 7:
            default:
                throw new Exception(STRS_ERR[0]);
            case 4:
                Vector log_dElem = new Vector();
                Vector ari_dElem = new Vector();
                while (expr.size() > 0) {
                    Object pdt = vPopFront(expr);
                    ari_dElem.removeAllElements();
                    if (isSymb(pdt)) {
                        Symb pst = (Symb) pdt;
                        if (pst.isCompOp()) {
                            int rightQ = 0;
                            while (log_dElem.size() > 0) {
                                Object pdt2 = vPopBack(log_dElem);
                                if (isSymb(pdt2)) {
                                    if (((Symb) pdt2).getVal() == 15) {
                                        rightQ++;
                                    }
                                    if (((Symb) pdt2).isLogicOp() || (((Symb) pdt2).getVal() == 14 && rightQ == 0)) {
                                        vPushBack(log_dElem, pdt2);
                                        while (ari_dElem.size() > 1) {
                                            calcExprNumImpl(ari_dElem);
                                        }
                                        if (ari_dElem.size() > 0) {
                                            vPushBack(log_dElem, vPopFront(ari_dElem));
                                        }
                                    } else if (((Symb) pdt2).getVal() == 14) {
                                        rightQ--;
                                    }
                                }
                                vPushFront(ari_dElem, pdt2);
                            }
                            while (ari_dElem.size() > 1) {
                                calcExprNumImpl(ari_dElem);
                            }
                            if (ari_dElem.size() > 0) {
                                vPushBack(log_dElem, vPopFront(ari_dElem));
                            }
                        }
                    }
                    vPushBack(log_dElem, pdt);
                    ari_dElem.removeAllElements();
                    if (isSymb(pdt) && ((Symb) pdt).isCompOp()) {
                        int leftQ = 0;
                        while (expr.size() > 0) {
                            Object pdt3 = vPopFront(expr);
                            if (isSymb(pdt3)) {
                                Symb pst2 = (Symb) pdt3;
                                byte opType = pst2.getVal();
                                if (opType == 14) {
                                    leftQ++;
                                }
                                if (pst2.isLogicOp() || (opType == 15 && leftQ == 0)) {
                                    vPushFront(expr, pdt3);
                                    while (ari_dElem.size() > 1) {
                                        calcExprNumImpl(ari_dElem);
                                    }
                                    if (ari_dElem.size() > 0) {
                                        vPushBack(log_dElem, vPopFront(ari_dElem));
                                    }
                                } else if (opType == 15) {
                                    leftQ--;
                                }
                            }
                            vPushBack(ari_dElem, pdt3);
                        }
                        while (ari_dElem.size() > 1) {
                            calcExprNumImpl(ari_dElem);
                        }
                        if (ari_dElem.size() > 0) {
                            vPushBack(log_dElem, vPopFront(ari_dElem));
                        }
                    }
                }
                while (log_dElem.size() > 1) {
                    calcExprLgcImpl(log_dElem);
                }
                Object resultDt2 = vPopBack(log_dElem);
                return resultDt2;
            case 8:
                Object resultDt3 = vPopBack(expr);
                return resultDt3;
        }
        while (expr.size() > 1) {
            calcExprNumImpl(expr);
        }
        Object resultDt4 = vPopBack(expr);
        return resultDt4;
    }

    private Vector preCalc(Vector expr, Hashtable varList) throws Exception {
        Object ob;
        Vector tgt = new Vector();
        while (expr.size() > 0) {
            String s = (String) vPopFront(expr);
            if (s.charAt(0) == '\"') {
                ob = new Str(s.substring(1, s.length() - 1));
            } else if (isSymbol(s.charAt(0))) {
                ob = new Symb(s);
                if (((Symb) ob).getVal() == 0) {
                    throw new Exception(String.valueOf(s) + STRS_ERR[8]);
                }
            } else if (isNumeric(s.charAt(0))) {
                ob = new Int(s);
            } else if (s.charAt(s.length() - 1) == ')') {
                ob = callSub(s, varList);
            } else if (s.charAt(s.length() - 1) == ']') {
                ob = _getArr(s, varList);
            } else if (Bool.isBool(s)) {
                ob = new Bool(s);
            } else {
                ob = _getVar(s, varList);
                if (ob == null) {
                    throw new Exception(String.valueOf(s) + STRS_ERR[4]);
                }
            }
            vPushBack(tgt, ob);
        }
        return tgt;
    }

    private void calcExprStrImpl(Vector expr) {
        StringBuffer sb = new StringBuffer();
        while (expr.size() > 0) {
            Object ts = vPopFront(expr);
            if (!(ts instanceof Symb)) {
                sb.append(((DataType) ts).getString());
            }
        }
        vPushFront(expr, new Str(sb.toString()));
    }

    private void calcExprNumImpl(Vector expr) {
        if (expr.size() != 1) {
            Object element1 = vPopFront(expr);
            if (element1 instanceof Symb) {
                if (((Symb) element1).getVal() == 14) {
                    calcExprNumImpl(expr);
                    Object element2 = vPopFront(expr);
                    Object element3 = vPopFront(expr);
                    if ((element3 instanceof Symb) && ((Symb) element3).getVal() == 15) {
                        vPushFront(expr, element2);
                        return;
                    }
                    vPushFront(expr, element3);
                    vPushFront(expr, element2);
                    vPushFront(expr, element1);
                    calcExprNumImpl(expr);
                    return;
                }
                if (((Symb) element1).getVal() == 1) {
                    calcExprNumImpl(expr);
                    return;
                }
                if (((Symb) element1).getVal() == 2) {
                    Object element4 = vPopFront(expr);
                    if (element4 instanceof Int) {
                        vPushFront(expr, new Int(-((Int) element4).getVal()));
                        return;
                    }
                    vPushFront(expr, element4);
                    calcExprNumImpl(expr);
                    vPushFront(expr, element1);
                    calcExprNumImpl(expr);
                    return;
                }
                return;
            }
            if (element1 instanceof Int) {
                Symb element5 = (Symb) vPopFront(expr);
                Object element6 = vPopFront(expr);
                if (element5.getVal() == 3 || element5.getVal() == 4) {
                    if (element6 instanceof Int) {
                        int n1 = ((Int) element1).getVal();
                        int n2 = ((Int) element6).getVal();
                        Int val = new Int(element5.getVal() == 3 ? n1 * n2 : n1 / n2);
                        vPushFront(expr, val);
                        return;
                    }
                    vPushFront(expr, element6);
                    calcExprNumImpl(expr);
                    vPushFront(expr, element5);
                    vPushFront(expr, element1);
                    calcExprNumImpl(expr);
                    return;
                }
                if (element5.getVal() == 1 || element5.getVal() == 2) {
                    boolean calc = false;
                    if (element6 instanceof Int) {
                        if (expr.size() == 0) {
                            calc = true;
                        } else {
                            Object element7 = vPopFront(expr);
                            if (element7 != null) {
                                if (((Symb) element7).getVal() != 3 && ((Symb) element7).getVal() != 4) {
                                    calc = true;
                                }
                                vPushFront(expr, element7);
                            }
                        }
                    }
                    if (calc) {
                        int n3 = ((Int) element1).getVal();
                        int n4 = ((Int) element6).getVal();
                        Int val2 = new Int(element5.getVal() == 1 ? n3 + n4 : n3 - n4);
                        vPushFront(expr, val2);
                        return;
                    }
                    vPushFront(expr, element6);
                    calcExprNumImpl(expr);
                    vPushFront(expr, element5);
                    vPushFront(expr, element1);
                    calcExprNumImpl(expr);
                    return;
                }
                if (element5.getVal() == 15) {
                    if (element6 != null) {
                        vPushFront(expr, element6);
                    }
                    vPushFront(expr, element5);
                    vPushFront(expr, element1);
                }
            }
        }
    }

    private void calcExprLgcImpl(Vector expr) {
        if (expr.size() != 1) {
            Object element1 = vPopFront(expr);
            if (element1 instanceof Symb) {
                if (((Symb) element1).getVal() == 14) {
                    calcExprLgcImpl(expr);
                    Object element2 = vPopFront(expr);
                    Object element3 = vPopFront(expr);
                    if ((element3 instanceof Symb) && ((Symb) element3).getVal() == 15) {
                        vPushFront(expr, element2);
                        return;
                    }
                    vPushFront(expr, element3);
                    vPushFront(expr, element2);
                    vPushFront(expr, element1);
                    calcExprLgcImpl(expr);
                    return;
                }
                if (((Symb) element1).getVal() == 11) {
                    Object element4 = vPopFront(expr);
                    if (element4 instanceof Bool) {
                        vPushFront(expr, new Bool(!((Bool) element4).getVal()));
                        return;
                    }
                    vPushFront(expr, element4);
                    calcExprLgcImpl(expr);
                    vPushFront(expr, element1);
                    calcExprLgcImpl(expr);
                    return;
                }
                return;
            }
            if (element1 instanceof Int) {
                int n1 = ((Int) element1).getVal();
                Symb operator = (Symb) vPopFront(expr);
                int n2 = ((Int) vPopFront(expr)).getVal();
                boolean result = false;
                if (operator.getVal() == 5) {
                    result = n1 > n2;
                } else if (operator.getVal() == 6) {
                    result = n1 < n2;
                } else if (operator.getVal() == 8) {
                    result = n1 >= n2;
                } else if (operator.getVal() == 9) {
                    result = n1 <= n2;
                } else if (operator.getVal() == 10) {
                    result = n1 != n2;
                } else if (operator.getVal() == 7) {
                    result = n1 == n2;
                }
                vPushFront(expr, new Bool(result));
                return;
            }
            if (element1 instanceof Bool) {
                Symb element5 = (Symb) vPopFront(expr);
                Object element6 = vPopFront(expr);
                if (element5.getVal() == 12) {
                    if (element6 instanceof Bool) {
                        boolean result2 = ((Bool) element1).getVal() && ((Bool) element6).getVal();
                        vPushFront(expr, new Bool(result2));
                        return;
                    }
                    vPushFront(expr, element6);
                    calcExprLgcImpl(expr);
                    vPushFront(expr, element5);
                    vPushFront(expr, element1);
                    calcExprLgcImpl(expr);
                    return;
                }
                if (element5.getVal() == 13) {
                    boolean calc = false;
                    if (element6 instanceof Bool) {
                        if (expr.size() == 0) {
                            calc = true;
                        } else {
                            Object element7 = vPopFront(expr);
                            if ((element7 instanceof Symb) && ((Symb) element7).getVal() != 12) {
                                calc = true;
                            }
                            vPushFront(expr, element7);
                        }
                    }
                    if (calc) {
                        boolean result3 = ((Bool) element1).getVal() || ((Bool) element6).getVal();
                        vPushFront(expr, new Bool(result3));
                        return;
                    }
                    vPushFront(expr, element6);
                    calcExprLgcImpl(expr);
                    vPushFront(expr, element5);
                    vPushFront(expr, element1);
                    calcExprLgcImpl(expr);
                    return;
                }
                if (element5.getVal() == 15) {
                    if (element6 != null) {
                        vPushFront(expr, element6);
                    }
                    vPushFront(expr, element5);
                    vPushFront(expr, element1);
                }
            }
        }
    }

    private Object callSub(String instruct, Hashtable varList) throws Exception {
        if (instruct == null) {
            return _sub(null, 0);
        }
        String paraVal = instruct.substring(instruct.indexOf(40) + 1, instruct.lastIndexOf(41));
        Vector paraStack = new Vector();
        if (paraVal.length() != 0) {
            Vector sValue = parseInstruct(paraVal);
            while (sValue.size() > 0) {
                StringBuffer exprStrBuf = new StringBuffer();
                while (sValue.size() > 0) {
                    String tmps = vPopFront(sValue).toString();
                    if (tmps.charAt(0) == ',') {
                        break;
                    }
                    exprStrBuf.append(tmps);
                }
                String exprStr = exprStrBuf.toString();
                Object v = calcExpr(exprStr, varList);
                if (v != null) {
                    vPushFront(paraStack, v);
                } else {
                    throw new Exception(String.valueOf(STRS_ERR[5]) + exprStr);
                }
            }
        }
        String subName = getFirstWord(instruct);
        if (subName.startsWith(FUNC_FLAG)) {
            return call(paraStack, Integer.parseInt(subName.substring(1), 16));
        }
        throw new Exception(STRS_ERR[3]);
    }

    private Vector parseArrayPos(String arrStr, Hashtable varList) throws Exception {
        Vector stack = new Vector();
        String arrStr2 = arrStr.substring(arrStr.indexOf(91));
        int leftQ = 0;
        StringBuffer sb = new StringBuffer();
        for (int i = 0; i < arrStr2.length(); i++) {
            char ch = arrStr2.charAt(i);
            if (ch == '[') {
                leftQ++;
            } else if (ch == ']') {
                leftQ--;
                Object o = calcExpr(sb.toString(), varList);
                if (o instanceof String) {
                    throw new Exception(STRS_ERR[2]);
                }
                vPushBack(stack, o);
                sb.setLength(0);
            } else {
                sb.append(ch);
            }
        }
        return stack;
    }

    private void _setArr(String instruct, Hashtable varList) throws Exception {
        Vector stack = parseInstruct(instruct);
        String leftStr = (String) vPopFront(stack);
        String arrName = getFirstWord(leftStr).toLowerCase();
        Vector paraStack = parseArrayPos(leftStr, varList);
        int deepth = paraStack.size();
        int[] dimPara = new int[deepth];
        for (int i = dimPara.length - 1; i >= 0; i--) {
            dimPara[i] = ((Int) vPopBack(paraStack)).getVal();
        }
        if (stack.size() == 0 || (stack.size() != 0 && ((String) vPopFront(stack)).charAt(0) == ':')) {
            varList.put(arrName, new Array(dimPara));
            return;
        }
        Object arr = varList.get(arrName);
        if (arr != null && (arr instanceof Array)) {
            String exprStr = instruct.substring(instruct.indexOf(61) + 1);
            ((Array) arr).setValue(dimPara, calcExpr(exprStr, varList));
        }
    }

    private Object _getArr(String arrExpr, Hashtable varList) throws Exception {
        String arrName = getFirstWord(arrExpr);
        String arrName2 = arrName.toLowerCase();
        Vector paraStack = parseArrayPos(arrExpr, varList);
        int deepth = paraStack.size();
        int[] dimPara = new int[deepth];
        for (int i = dimPara.length - 1; i >= 0; i--) {
            dimPara[i] = ((Int) vPopBack(paraStack)).getVal();
        }
        Object arr = varList.get(arrName2);
        if (arr != null && (arr instanceof Array)) {
            return ((Array) arr).getValue(dimPara);
        }
        return null;
    }

    public static void setValue(String key, Int value) {
        globalValueHT.put(key, value);
    }

    public static Int getValue(String key) {
        return globalValueHT.containsKey(key) ? (Int) globalValueHT.get(key) : new Int(0);
    }

    public static Object[] getAllValues() {
        if (globalValueHT.size() == 0) {
            return null;
        }
        System.out.println("globalValueHT.size() = " + globalValueHT.size());
        Object[] obs = new Object[globalValueHT.size() * 2];
        Enumeration em = globalValueHT.keys();
        int count = 0;
        while (em.hasMoreElements()) {
            String key = (String) em.nextElement();
            obs[count * 2] = key;
            obs[(count * 2) + 1] = globalValueHT.get(key);
            count++;
        }
        System.out.println("count = " + count);
        return obs;
    }

    public DataType call(Vector para, int methodID) throws Exception {
        switch (methodID) {
            case 0:
                return print(para);
            case 1:
                return println(para);
            case 2:
                return min(para);
            case 3:
                return max(para);
            case 4:
                return asize(para);
            case 5:
                return abs(para);
            case 6:
                return random(para);
            case 7:
                return mod(para);
            case 8:
                return ssize(para);
            case ERR_ARR_OUT /* 9 */:
                setValue(vPopStringBack(para), (Int) vPopBack(para));
                return null;
            case 10:
                return getValue(vPopStringBack(para));
            default:
                return this.scriptProcessor.call(para, methodID);
        }
    }

    private DataType print(Vector para) {
        String s = ((DataType) vPopBack(para)).getString();
        System.out.print(s);
        return null;
    }

    private DataType println(Vector para) {
        DataType dt = (DataType) vPopBack(para);
        String s = dt == null ? null : dt.getString();
        if (s == null) {
            System.out.println();
        } else {
            System.out.println(s);
        }
        return null;
    }

    private Int min(Vector para) {
        int x = vPopIntBack(para);
        int y = vPopIntBack(para);
        if (x <= y) {
            y = x;
        }
        return new Int(y);
    }

    private Int max(Vector para) {
        int x = vPopIntBack(para);
        int y = vPopIntBack(para);
        if (x <= y) {
            x = y;
        }
        return new Int(x);
    }

    private Int asize(Vector para) {
        Array arr = (Array) vPopBack(para);
        return new Int(arr.getLength());
    }

    private Int abs(Vector para) {
        int x = vPopIntBack(para);
        return new Int(Math.abs(x));
    }

    public static DataType random(Vector para) {
        return new Int(Tool.getNextRnd(vPopIntBack(para), vPopIntBack(para)));
    }

    private Int mod(Vector para) {
        int x = vPopIntBack(para);
        int y = vPopIntBack(para);
        return new Int(x % y);
    }

    private Int ssize(Vector para) {
        String s = vPopBack(para).toString();
        return new Int(s.length());
    }
}
