const DEFAULTURL = "../center.html";
const LOGINURL = //"http://pt.cs0309.3g.qq.com/login?act=json&sidtype=0&bid_code=minigameLogin";
	"http://pt.3g.qq.com/login?act=json&sidtype=0&bid_code=minigameLogin";
var firstValid = true;

var promptEl = "ptlogin-prompt";

if ('undefined' == typeof SUPPORTTOUCH)
{
    var SUPPORTTOUCH = ("createTouch" in document);
}

if ('undefined' == typeof TOUCHSTARTEVENT)
{
    var TOUCHSTARTEVENT = SUPPORTTOUCH ? "touchstart" : "mousedown";
}

if ('undefined' == typeof ORIENTATIONCHANGE)
{
  var ORIENTATIONCHANGE = ("onorientationchange" in window);
}

if ('undefined' == typeof ORIENTATIONEVENT)
{
  var ORIENTATIONEVENT = ORIENTATIONCHANGE ? "orientationchange" : "resize";
}

window.addEventListener("DOMContentLoaded",main,false);

function main(){
    event_init();
    var pEl = document.querySelector("#" + promptEl);
    pEl.style.left = "50%";
    var widthStr = window.getComputedStyle(pEl).width;
    pEl.style.marginLeft = "-" + parseInt(widthStr.substring(0,widthStr.length - 2))/2 + "px";
    var lockEl = document.querySelector(".load-box");
    lockEl.style.left = "50%";
    widthStr = window.getComputedStyle(lockEl).width;
    lockEl.style.marginLeft = "-" + parseInt(widthStr.substring(0,widthStr.length - 2))/2 + "px";
}
/*初始化事件*/
function event_init(){
   var emptyBtns = document.querySelectorAll(".empty");
   for (var i = 0; i <  emptyBtns.length; i++)
   {
       emptyBtns[i].addEventListener(TOUCHSTARTEVENT,clearContent,false);
   }
   
   var inputs = document.querySelectorAll(".loginfiled input");
   for (var i = 0; i <  inputs.length; i++)
   {
       inputs[i].addEventListener('keyup',onKeyUp,false);
   }

   document.querySelector("#ptlogin-submit").addEventListener("click",login,false);
   document.querySelector("#ptlogin-relog").addEventListener("click",relog,false);
   document.querySelector("#ptlogin-lock").addEventListener(TOUCHSTARTEVENT,function(e){e.preventDefault()},false);
   document.querySelector(".img-varify img").addEventListener("click",loadImg,false);
   document.querySelector(".img-varify img").onerror = function(){
        hideValidCode();
        showPrompt("验证超时请重新登录!");
   }
   window.addEventListener(ORIENTATIONEVENT,orientChangeHandler,false);
}

function login(){
    var user = encodeURIComponent(trim(document.querySelector("#ptlogin-name").value));
    var oriPwd = document.querySelector("#ptlogin-password").value.substr(0,16);
    
    if (!window.w_is_qq_service) {savePasswdToQQService(user, document.querySelector("#ptlogin-password").value);}
    var pwd =  window.w_is_qq_service? document.querySelector("#ptlogin-password").value : hex_md5(oriPwd);

    var url = LOGINURL + "&qq=" + user + "&pmd5=" + pwd;

    var veryCode = document.querySelector("#ptlogin-imgVerify").value;
    var r_sid = null;
    r_sid = document.querySelector(".img-varify img").getAttribute("r_sid");
    var u_token = null
    u_token = document.querySelector(".img-varify img").getAttribute("u_token");
    	
    if ('' != veryCode && r_sid && '' != r_sid)
    {
        url += "&u_token=" + u_token + "&verify=" + veryCode + "&r_sid=" + r_sid;
    }

    
    //console.log(url);
    showLoading();
    request_script("script_log_res",url,"resHandler");
}

function relog(){
    hidePrompt();
    hideLock();
    document.querySelector("#ptlogin-imgVerify").value = "";
}

function clearContent(e){
    e.preventDefault();
    var target = e.target;
    var input = target.parentNode.querySelector("input");
    input.value = "";
    target.style.display = 'none';
}

function showLoading(){
    var el = document.querySelector(".load-box");
    el.style.display = "block";
    //el.style.top = (document.documentElement.clientHeight - el.clientHeight)/2 + window.pageYOffset + "px";
    el.style.top = document.querySelector("#title").clientHeight + "px";
    showLock();
}

function hideLoading(){
    var el = document.querySelector(".load-box");
    el.style.display = "none";
}

function showLock(){
    var el = document.querySelector("#ptlogin-lock");
    el.style.display = 'block';
    el.style.height = Math.max(document.documentElement.offsetHeight,document.documentElement.clientHeight) + "px";
}

function hideLock(){
    var el = document.querySelector("#ptlogin-lock");
    el.style.display = "none";
}

function showValidcode(res){ 
    document.querySelector("#ptlogin-imgVerify").value = "";
    var el = document.querySelector(".varify-wrapper");
    el.style.display = "block";
    loadImg(null,res);
}

function hideValidCode(){
    var el = document.querySelector(".varify-wrapper");
    document.querySelector("#ptlogin-imgVerify").value = "";
    el.style.display = "none";
    firstValid = true;
}

function loadImg(e,res){
    var el = document.querySelector(".varify-wrapper");
    var random = Math.random();
    var img = el.querySelector("img");
    var src = ""
    var osrc = "";
    if (!res)
    {
        var osrc = img.getAttribute("osrc");
        img.setAttribute("src",osrc + "?" + Math.floor(random*10000));
    }
    else{
        osrc = res.vurl
        src = osrc; 
        img.setAttribute("r_sid",res.rsid);
        img.setAttribute("u_token",res.u_token);
        img.setAttribute("src",src + ".jpg" + "?" + Math.floor(random*10000));
        img.setAttribute("osrc",src+ ".jpg");
    }

    el.style.display = "block";
}

function showPrompt(str){
    var el = document.querySelector("#" + promptEl);
    el.querySelector("h3").innerHTML = str;
    el.style.display = "block";
    adjustPos();
}

function hidePrompt(){
    var el = document.querySelector("#" + promptEl);
    el.style.display = "none";
}

function adjustPos(){
    var el = document.querySelector("#" + promptEl);
    //el.style.top = (document.documentElement.clientHeight - el.clientHeight)/2 + window.pageYOffset + "px";
    //showPrompt(document.querySelector("#title").clientHeight + "," + (document.documentElement.clientHeight - el.clientHeight)/2);
    el.style.top = Math.min(document.querySelector("#title").clientHeight,(document.documentElement.clientHeight - el.clientHeight)/2) + window.pageYOffset + "px";
}

function request_script(id,url,callback){
    var random = Math.random();
    url += "&callback=" + callback;
    url += "&r=" + Math.floor(random*10000);

    var oScript = document.getElementById(id);
    var head = document.getElementsByTagName("head").item(0);
    if (oScript) {
        head.removeChild(oScript);
    }
    var nScript = document.createElement("script");
    nScript.setAttribute("src", url);
    nScript.setAttribute("id",id);
    nScript.setAttribute("type","text/javascript");
    nScript.setAttribute("language","javascript");
    head.appendChild(nScript);
    return nScript;


}

function orientChangeHandler(){
    var el = document.querySelector("#ptlogin-lock");
    if ('block' == el.style.display)
    {
        el.style.height = Math.max(document.documentElement.offsetHeight,document.documentElement.clientHeight) + "px";
        window.setTimeout(0,adjustPos);
    }
}

function resHandler(res){
    hideLoading();
    
    if (0 == res.code) {
        
        var oScript = document.getElementById("script_login");
        var head = document.getElementsByTagName("head").item(0);
        if (oScript) {
            head.removeChild(oScript);
        }
        
        /*过滤backurl中的sid*/
        var url = BACKURL||DEFAULTURL;
        var hash = (RegExp('(\\#.*)').exec(url)||[,''])[1];
        url = url.replace(hash,"");
        var re0 = /&amp;/gi;
        url = url.replace(re0,"&");
        var re = /&sid=[^&]*&|sid=[^&]*&/gi
        url = url.replace(re,"&")
        var re1 = /&sid=.*|sid=.*/gi
        url = url.replace(re1,"")
        var index = url.indexOf("?");
        url = (-1 == index ? url + "?" : url + "&");
        
        window.location.href = url + "sid=" + res.sid + hash;
    }
    else {
        if (40001 == res.code) {
            if (!firstValid)
            {
                showPrompt(res.msg);
            }
            else{
                firstValid = false;
                hideLock();
            }
            
            showValidcode(res);
        }
        else {
            hideValidCode();
            showPrompt(res.msg);
        }
    }
}

function trim(str){  
    return str.replace(/(^\s*)|(\s*$)/g, "");  
}

function onKeyUp(e){
	var target = e.target;
	var value = target.value;
    var cbtn = target.parentNode.querySelector(".empty");
	if (0 < value.length)
	{
		if ('block' != cbtn.style.display)
		{
			cbtn.style.display = 'block';
		}
	}
	else{
		if ('none' != cbtn.style.display)
		{
			cbtn.style.display = 'none';
		}
	}
}