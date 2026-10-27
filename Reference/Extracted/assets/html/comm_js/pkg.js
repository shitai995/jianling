/**
 * p_pkg 软件管理
 */

var p_pkg = window.p_pkg || {};

/**
 * 是否来自android终端
 */
p_pkg.isFromAndroidClient = function() {
	try{
		return typeof(window.comtencentwebnetJavaScript) != "undefined" && typeof(window.comtencentwebnetJavaScript.IsAndriodSDK) != "undefined";
	}catch(e)
	{
		return false;
	}
	
};



//fromiossdk
/**
 * 是否来自iphone终端
 */
function getUrlPara(name, defVal) { return (RegExp('(\\?|&)' + name+ '=([^&]*)').exec(location.search)||[,,undefined==defVal ? '' : defVal])[2]; }
p_pkg.isFromIPhoneClient = function() { return getUrlPara("fromiossdk").length > 0; };


//p_pkg.isFromIPhoneClient();
//alert("hi");




/**
 * 清空回退历史
 */
p_pkg.clearHistory = function() {
	//alert('before send sms');
	if (p_pkg.isFromAndroidClient()) {
		return window.comtencentwebnetJavaScript.ClearHistory();
	}
};



p_pkg.syncMicroblogging = function(msg, url) {
	
	
	if (p_pkg.isFromAndroidClient()) {
		return window.comtencentwebnetJavaScript.SyncMicroblogging(msg, "wdw");
	}
};






/**
 * 终端弹出提示语
 */
p_pkg.showMsg = function(text) {
	
	if (p_pkg.isFromAndroidClient()) {
		window.comtencentwebnetJavaScript.ShowMessage(text);//来自android终端，调用本地tips
	}
	else if (p_pkg.isFromIPhoneClient()) {
		p_util.invokeIPhoneFunc("showMessage", text);
	}
	else { alert(text); }
	
	
};


/**
 * 发送短信接口
 */
p_pkg.sendSMS= function(number, text) {
	alert('before send sms');
	return window.comtencentwebnetJavaScript.SendSMS(number, text);
};

/**
 * 发送短信的回调函数
 */
p_pkg.sendSMSCB = function() { }; //使用时自己注册回调事件



/**
 * 跳转本地页面接口
 */
p_pkg.toPage = function(url) {
	//alert('before ToPage: ' + url)
	return window.comtencentwebnetJavaScript.ToPage(url);
};




/**
 * 获取数据
 * @param key - data key
 */
p_pkg.getData = function(key) {
	//alert(apkName);
	if (p_pkg.isFromAndroidClient()) {
		var value = window.comtencentwebnetJavaScript.GetData(key);
		return new String(value);
	} 
	else {
		return "";
	}
};


/**
 * 设置数据
 * @param key - data key
 * @param value - data value
 */
p_pkg.setData = function(key, value) {
	//alert(apkName);
	if (p_pkg.isFromAndroidClient()) {
		window.comtencentwebnetJavaScript.SetData(key, value);
	} 
	
};




/**
 * getGpsInfo
 * @retrun - json string
 * 
 * 
 */
p_pkg.getGpsInfo = function() {
	return window.comtencentwebnetJavaScript.GetGpsInfo();
};
//返回json格式

//｛
//	"iX":	int,	/* 经度 */
//	"iY":	int,	/* 纬度 */
//	"iMCC":	int,	/* 国家码 */
//	"iMNC":	int,	/* 网络代码：移动、联通等 */
//	"iLAC":	int,	/* 地区码 */
//	"iCellId":	int,	/* 基站ID */
//	"clientIp":	"string",	/* 用户出口IP */
//	"wifiMac":	"string"	/* 用户Wifi路由Mac地址 */
//	
//｝;





/**
 * get terminal info
 * @retrun - json string
 * 
 */
p_pkg.getTermInfo = function() {
	return window.comtencentwebnetJavaScript.GetTermInfo();
};
//返回json的格式
//var abdef = ｛
//	"OSName":	"string",	/* 经度 */
//	"OSVer":	"string",	/* 纬度 */
//	"term":	"string",		/* 国家码 */
//	"resolution":	"string"/* 分辩率 */
//｝;



/*
 * cp调用登陆界面的回调
 * login call back
 */
p_pkg.loginCB = function(rst) {
	return window.comtencentwebnetJavaScript.LoginCB(rst);
};




/*
 * 计费点回调
 * pay call back
 */
p_pkg.payCB = function(id, rst) {
	//p_pkg.showMsg('payCB');
	return window.comtencentwebnetJavaScript.PayCB(id, rst);
};

/**
 * 返回游戏
 */
p_pkg.runSelf = function() {
	//p_pkg.showMsg('run:self');
	console.log('run:self');
	return window.comtencentwebnetJavaScript.RunSelf();
}

/**
 * 启动终端软件
 */
p_pkg.runApk = function(apkName) {
	console.log('run:'+apkName);
	return window.comtencentwebnetJavaScript.RunApk(apkName);
};

p_pkg.isClientDownValid = function() {
	var isValid = (p_pkg.isFromAndroidClient() && typeof(window.comtencentwebnetJavaScript.download) != "undefined");
	console.log('p_pkg.isClientDownValid:' + isValid);
	return isValid;
};

/**
 * 上报下载
 */
p_pkg.download = function(reportUrl, downUrl, gameName, pkgName, icon, version) {
	console.log('p_pkg.download:'+ reportUrl + downUrl + gameName + pkgName + icon + version);
	return window.comtencentwebnetJavaScript.download(reportUrl, downUrl, gameName, pkgName, icon, version);
}


/**
 * 
 * @param apks - arrays of apk name
 * @return	string - 反回值代表游戏包是否安装：1代表安装，0代表未安装。每个apk占一个字符。
 */
p_pkg.checkApk = function(apks) {
	//alert('checkApk:'+apks);
	console.log(apks);
	if(apks == '') {
		return 0;
	}
	
	var ret = 0;
	
	
	if (p_pkg.isFromAndroidClient()) {
		ret = new String(window.comtencentwebnetJavaScript.CheckApk(apks));
		console.log(ret);
	}
	
	
	//alert(ret);
	return ret == '1';
};



/**
 * 
 * @param apkName - apk p_pkg name
 */
p_pkg.getApkVersion = function(p_pkgName) {
	//alert(apkName);
	if (p_pkg.isFromAndroidClient()) {
		return window.comtencentwebnetJavaScript.GetApkVersion(p_pkgName);
	} 
	else {
		return "";
	}
};

/**
 * 取版本号
 */
p_pkg.getBuildVersion = function() {
	
	if (p_pkg.isFromAndroidClient()) {
		var ret = window.comtencentwebnetJavaScript.GetBuildVersion();
		console.log('buildVersion:'+ret);
		return new String(ret);
	} 
	else {
		return "";
	}
}

/**
 * 
 * @param apkName - apk name
 */
p_pkg.installApk = function(url) {
	//alert(apkName);
	return window.comtencentwebnetJavaScript.InstallApk(url);
};

/**
 * 
 * @param new mail num
 */
p_pkg.SetMessageNum = function(num) {
	if( !p_pkg.isFromAndroidClient() || typeof(window.comtencentwebnetJavaScript.SetMessageNum) == "undefined" )
		return;
	console.log('SetMessageNum:'+num);
	return window.comtencentwebnetJavaScript.SetMessageNum(num);
}

p_pkg.installProcess = function(resId, process) {
	//alert(resId + "|" + process);
	$li = $('input.res-id[value=' + resId + ']').parent();
	$li.find('.free').html(process);
	//alert($li);
};

p_pkg.getCpId = function() {
	if(window.location.href.match(/^http/)) {
		return 0;
	}
	return p_pkg.getData("CPID");
}

p_pkg.getGameId = function() {
	if(window.location.href.match(/^http/)) {
		return 902;
	}
	return p_pkg.getData("GAMEID");
}

p_pkg.isCurGame = function(scpId, sgameId, pkgName) {
	if (!p_pkg.isFromAndroidClient()) {
		return;
	}
	
	var curCpId = new Number(p_pkg.getCpId()).toString(); //当前调用的游戏
	var curGameId = new Number(p_pkg.getGameId()).toString();
	var iCpId = new Number(scpId).toString();
	var iGameId = new Number(sgameId).toString();
	var isCurGame = (curCpId == iCpId && curGameId == iGameId);
	
	console.log("curCpId: " + curCpId);
	console.log("curGameId: " + curGameId);
	console.log("cpId: " + iCpId);
	console.log("gameId: " + iGameId);
	console.log("isCurGame: " + isCurGame);
	
	return isCurGame;
}



p_pkg.startGame = function(cpId, gameId, pkgName) {
	if (!p_pkg.isFromAndroidClient()) {
		return;
	}
	
	if (p_pkg.isCurGame(cpId, gameId, pkgName)) {
		p_pkg.runSelf();
	}
	else {
		p_pkg.runApk(pkgName);
	}
	
};









/*

调用终端的函数
public void RunApk(final String p_pkg)

public void CheckApk(final String[] p_pkg)
		
public void GetApkVersion(final String p_pkg)
		
public void InstallApk(final String p_pkgUrl)

public void PushData(final String key, Object value)
		
public void GetData(final String key)
		
public void ActivationSuccessful()

*/