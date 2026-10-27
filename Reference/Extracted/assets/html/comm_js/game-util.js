/**
 * @author devinwang
 */
/*sid=AQToNeqp1dIhhVhrtPO61s_u

cpId=00&gameId=805&sid=AQToNeqp1dIhdl00fTNg1Phs

*/


var g_version = 0;
var g_refresh_interval = 20;
var g_platform = "browser";
var g_qq_domain = "qq.com";

var onCls = 'on';
var anchors = window.document.anchors;
var jsonData = '';
var cpId = p_util.getUrlQueryPara('cpId', '00');
var gameId = p_util.getUrlQueryPara('gameId', '902');
var isPreload = p_util.getUrlQueryPara('isPreload', '0');

const ACTION_ID_BECOME_FRIEND       = 153;
const ACTION_ID_COMMENT_GAME        = 154;
const ACTION_ID_GAIN_ACHIVEMENT     = 152;
const ACTION_ID_GAIN_HIGH_SORT      = 151;
const ACTION_ID_PLAY_GAME           = 150;
const ACTION_ID_SIGNATURE           = 205;

const HOT_RANK_ID                   = 1; //热门榜单
const NEW_RANK_ID                   = 2; //新品榜单
const REMARK_RANK_ID                = 8; //评分榜单

console.log('simple.js by devin');
console.log('current version:' + g_version);
console.log('window.location: ' + window.location); 


console.log('param isPreload: '+isPreload);

window.iphone = window.iphone || {};
var p_pkg = window.p_pkg || {};
//fromiossdk
/**
* 是否来自iphone终端
*/
function getUrlPara(name, defVal) { return (RegExp('(\\?|&)' + name+ '=([^&]*)').exec(location.search)||[,,undefined==defVal ? '' : defVal])[2]; }
p_pkg.isFromIPhoneClient = function() { return getUrlPara("fromiossdk").length > 0; };



if (p_util.getUrlQueryPara("fromiossdk").length > 0) {
	
	window.g_iphone_func_img = document.createElement("img");
	window.g_iphone_func_img.style["display"] = "none";
	document.body.appendChild(window.g_iphone_func_img);
	
	var g_iphone_time = new Date().getTime();
	var g_iphone_count = 0;
	
	p_util.getIPhoneFuncRand = function(){
		return g_iphone_time + "_" + g_iphone_count++;
	};
}

/***
 * 页面是否为预加载
 */
p_util.isPreLoad = function() {
	console.log('func isPreload: '+isPreload);
	return isPreload == 1;
}

/***
 * 调用iphone方法
 * @author dingweiwang
 */
p_util.invokeIPhoneFunc = function() {
	//http://js//setData:sid:XXXXXX//随机数
	
	var argLen = arguments.length; 
	if (argLen < 1) {
		alert("invokeIPhoneFunc err");
	}
	var locStr = "http://js//" + arguments[0];
	
	for (var i = 1; i < argLen; i++) {
		locStr += ":" + new String(arguments[i]).replace(/:/g, "");
	}
	locStr += "//" + p_util.getIPhoneFuncRand();
	
	//alert("begin:" + locStr + "|" + window.g_iphone_func_img.getAttribute("src"));
	//window.location = "locStr";
	window.g_iphone_func_img.setAttribute("src", locStr);
	//alert("begin:" + locStr + "|" + window.g_iphone_func_img.getAttribute("src"));
	
}

/**
 * Iphone客户端，重定向到日志中
 */
if (p_util.getUrlQueryPara("fromiossdk").length > 0) {
	p_util.invokeIPhoneFunc("showLog", 'invoke showLog');
	window.console.log=function(text){
		p_util.invokeIPhoneFunc("showLog", text);
	};
}













// ^^^ 处理sid
var sid = p_util.getUrlQueryPara('sid');//sid参数一般取不到  //'AT1oZe1AgiRMJCWPjjrYihn0');//'AUkchq8VfIUyjPtKjj6P3zor');// 
console.log('sid1:' + sid);


//============== android ===============
if (typeof(window.comtencentwebnetJavaScript) != "undefined" 
	&& typeof(window.comtencentwebnetJavaScript.IsAndriodSDK) != "undefined") { //来自andoird终端
	
	g_platform = p_util.getUrlQueryPara("platform", "android");
	
	if (sid.length < 8) {//sid格式错误或不存在未登陆
		//console.log('sid2:' + sid);
		sid = window.comtencentwebnetJavaScript.GetData('sid');//取终端的数据
		sid = new String(sid);
		//console.log('sid3:' + sid);
	}
	else {
		//console.log('sid4:' + sid);
		
		if ('false' != p_util.getUrlQueryPara('l_lgcb', 'false')) {//登陆回调
			
			window.comtencentwebnetJavaScript.SetData('sid', sid);
			window.comtencentwebnetJavaScript.Login(); //通知终端登陆成功
		}
	
		//console.log('sid5:' + sid);
	}

}
//============== android ===============
else if (p_util.getUrlQueryPara("fromiossdk").length > 0) { //iphone sid处理
	
	g_platform = p_util.getUrlQueryPara("platform", "iphone");
	
	if (sid.length < 8) {//sid格式错误或不存在未登陆,iphone的sid取自参数，这里不用做额外处理

	}
	else {
		//console.log('sid4:' + sid);
		
		//alert("ipone else:");
		
		
		if ('false' != p_util.getUrlQueryPara('l_lgcb', 'false')) {//登陆回调
			p_util.invokeIPhoneFunc("setData", "sid", sid);
			p_util.invokeIPhoneFunc("login", "sid");
		}
	
		//console.log('sid5:' + sid);
	}
	
}
else {//非终端时取cookie
	g_platform = p_util.getUrlQueryPara("platform", "browser");
	if (sid.length < 8) {//从cooke取
		sid = p_util.cookie.get('w_sid');
	}
	else {
		if ('false' != p_util.getUrlQueryPara('l_lgcb', 'false')) {//登陆回调
			p_util.cookie.set('w_sid', sid);//存sid到cookie
		}
	}
}
//alert(sid);

//console.log('sid6:' +  sid);
// $$$ 处理sid


if (window.location.href.match(/^http/)) {//
	console.log('http');
	window.g_serverBaseUrl = 'http://' + window.location.host + '/g/';
}
else {
	//window.g_serverBaseUrl = 'http://devg.cs0309.3g.qq.com/g/';
	window.g_serverBaseUrl = 'http://andsngame.3g.qq.com/g/';
}
console.log('serverUrl', window.g_serverBaseUrl);

window.g_resBaseUrl = "http://res.g.qq.com:8899/resource/";









/**********************************************************
					begin util func 
 **********************************************************/ 

// 取url参数中的cpId, gameId
// g_resBaseUrl - 单位为分钟
p_util.getUrl = function(s, a_refreshInterval) {
	
	//return window.g_serverBaseUrl + s + "?sid=" + sid + "&g_v=" + g_version +  "&cpId=" + cpId + "&gameId=" + gameId;
	return p_util.getUrlEx(s, cpId, gameId, a_refreshInterval);
};

p_util.getUrlEx = function(s, iCpId, iGameId, a_refreshInterval) {
	var  value = 0;
	if (p_pkg.isFromAndroidClient()) {
		value = p_pkg.getData(s);
		console.log("getUrlEx1:" + s + "|" + value);
		value = parseInt(value);
		var sec = Math.floor(new Date().getTime()/1000);//当前的秒
		if(!/\d/.test(value) || value  < 1000 || (sec - value) > a_refreshInterval*60) {// 需要刷新，用当前时间重新生成rnd
			p_pkg.setData(s, sec); //存储当前sec
			value = sec;
		}
		console.log("getUrlEx2:" + value);
	}
	
	var sUrl =  window.g_serverBaseUrl + s + "?sid=" + sid + "&g_v=" + g_version + "&cpId=" + iCpId + "&gameId=" + iGameId
	+ '&platform=' + g_platform + '&sw=' + window.screen.width + '&sh=' + window.screen.height//客户端版本，为空时当做浏览器过来的请求
	+ ('&_rnd=' + value); //缓存 
	
	console.log("server_url: " + sUrl);
	
	return	sUrl;
};

//http://211.139.188.52:8899/resource/mbox/appLogo/MA2010120200053_YL1.jpg
p_util.getLogoUrl = function(logo) {
	
	return  window.g_resBaseUrl + 'mbox/appLogo/' + logo;
};

p_util.getAchLogoUrl = function(resId, achId) {
	
	return  window.g_resBaseUrl + 'mbox/snlogo/'+ resId.substring(2,10)+'/'+resId+'/'+resId+'_chengjiu' + achId + '.png';
};

p_util.getAvatarUrl = function(avatarId) {
	if(avatarId.indexOf('http') >= 0) {
		return avatarId;
	}
	avatarId = avatarId == '' ?  1 : ( (((avatarId / 3 + 1) >= 100) ? 1 : ((avatarId) / 3 + 1)) );
	return  window.g_resBaseUrl + 'qqface/' + avatarId + '.jpg';//'img/tmp/head.jpg';//
};

p_util.getDownCount = function(downCount) {
	if(downCount > 10000)	{
		var fmtNum=new Number(downCount/10000);
		downCount = fmtNum.toFixed(2) + '万';
	}
	return downCount;
};

p_util.getFileSize = function(fileSize) {
	if(fileSize > 10485)	{
		var fmtNum=new Number(fileSize/1048576);
		return fmtNum.toFixed(2) + 'M';
	}
	else {
		var fmtNum=new Number(fileSize/1024);
		return fmtNum.toFixed(2) + 'K';
	}
}

p_util.setPageVar = function(name, value) {
	p_util.cookie.set(name, value, '', window.location.pathname);
}

p_util.getPageVar = function(name, defaultVal) {
	var ret = p_util.cookie.get(name) || defaultVal;
	
	return ret;
}

p_util.setPageNo = function(value) {
	p_util.setPageVar('pageno', value);
}

p_util.getPageNo = function(defaultVal) {
	return p_util.getPageVar('pageno', defaultVal);
}

function rad(d)
{
	return d * Math.PI / 180.0;
}

// 根据经纬度计算距离
p_util.getDistance = function(lng1, lat1, lng2, lat2) {
	if(lat1 == '' || lng1 == '' || lat2 == '' || lng2 == '' || (lat1 == 0 && lng1 == 0 )|| (lat2 == 0 && lng2 == 0)) {
		return "";
	}
	var EARTH_RADIUS = 6378.137;
	var radLat1 = rad(lat1);
	var radLat2 = rad(lat2);
	var a = radLat1 - radLat2;
	var b = rad(lng1) - rad(lng2);
	var s = 2 * Math.asin(Math.sqrt(Math.pow(Math.sin(a/2),2) + 
	    Math.cos(radLat1)*Math.cos(radLat2)*Math.pow(Math.sin(b/2),2)));
	s = s * EARTH_RADIUS;
	s = Math.round(s * 10000) / 10000;
	if(s> 1) {
		return new Number(s).toFixed(0) + "公里以内";
	}else if(s> 0.1)  {
		return new Number(s*10).toFixed(0) + "00米以内";
	}else  {
		return "100米以内";
	}
	
}

// 列表项的整个list可以点击
p_util.liClick = function(elem) {
	console.log(elem);
	
	window.location.href = $(elem).find('a').attr('href');
};


//强制调到登陆页面
p_util.login = function(id) {
	//loginserver的服务器地址
	//var  loginServerUrl = 'http://aarizzhang.kf0309.3g.qq.com/login/wap30/index.jsp?bid=2&backurl='; //传入backurl
	var loginServerUrl = window.location.href.match(/^http/) ? g_serverBaseUrl + 'androidNew/login/login.html?l_lgcb=true&backurl=' : './login/login.html?backurl=';
	
	//登陆成功回调的服务器地址
	
	var pkgUrl = window.location.href;
	pkgUrl = pkgUrl.indexOf('?') >= 0 ? pkgUrl += '&l_lgcb=true' : pkgUrl += '?l_lgcb=true'; //加上l_lgcb参数，标识登陆回调
	
	var loginUrl = loginServerUrl + encodeURIComponent(pkgUrl);
	console.log(loginUrl);
	window.location = loginUrl;
	
};


//当sid不正确时，设置到登界面
p_util.loginIfNotHasSid = function(id) {
	
	if (sid.length < 8) {//使用全局的sid
		p_util.login(id);
	}
	
};


//上报游戏，并重定向到下载页
p_util.reportAndDownload = function (downUrl, name, pkgName, logo, version)
{
	var reportDownUrl = p_util.getUrl('android/down.g', 0) + '&fromPage=new&goUrl=' + encodeURIComponent(downUrl);
	console.log('reportAndDownload: '+reportDownUrl);
	
	if(p_pkg.isClientDownValid()) {
		p_pkg.download(reportDownUrl, downUrl, name, pkgName, p_util.getLogoUrl(logo), '-1');
	}else {
		window.location.href = reportDownUrl;
	}
	
	return false;
}



//是否允许点击
p_util.enableClick = function (iCpId, iGameId)
{
	// 斗地主
	if(iCpId == 0 && iGameId == 3002) {
		return false;
	}
	else {
		return true;
	}

}
