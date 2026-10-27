/**
 * 3g的登陆代码存在本地，该js为适应业务做的修改 
 *
 */



if(window.comtencentwebnetJavaScript) {
	window.comtencentwebnetJavaScript.ClearHistory();
}



const BID = "2"; 
const BACKURL = decodeURIComponent(p_util.getUrlQueryPara('backurl', "../center.html"));
console.log(BACKURL);


function setUserPass(usr, passwd) {
	console.log(usr);
	console.log(passwd);
	window.w_is_qq_service = true;
	window.$usr.val(usr);
	window.$passwd.val(passwd);
}

function savePasswdToQQService(usr, passwd)
{
	if(window.comtencentwebnetJavaScript && window.comtencentwebnetJavaScript.SaveUserPass) {
		window.comtencentwebnetJavaScript.SaveUserPass(usr, passwd);
	}
}

window.w_is_qq_service = false;

function w_qqService() {
	$("body").append('<div id="w-qq-service" style="position:absolute; right:10px; top:10px; width: 58px; height: 58px; background:url(../img/QQ_tongxingzheng_APKbutton_normal.png.png) no-repeat top center"></div>');

	window.$usr = $('#ptlogin-name');
	window.$passwd = $('#ptlogin-password');
	
	$("#w-qq-service").click( function() {
		window.comtencentwebnetJavaScript.GetOtherUser();
	});
	
	$('input').keydown(function(evtO) {//输入
		window.w_is_qq_service = false;
	});
	
	var usr_passwd = new String(window.comtencentwebnetJavaScript.GetDefaultUser());
	console.log(usr_passwd);
	if (usr_passwd.length > 4) { //从qqservice中取出密码
		window.w_is_qq_service = true;
		var _tmp = usr_passwd.splitString(",");
		$usr.val(_tmp[0]);
		$passwd.val(_tmp[1]);
	}
}


$(document).ready(function() {
	

	if(window.comtencentwebnetJavaScript && window.comtencentwebnetJavaScript.UseQQService) {
		if ( "true" == new String(window.comtencentwebnetJavaScript.UseQQService()) ) {
			w_qqService();
		}
		
	}
	
	
	

	
});

