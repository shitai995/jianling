


var uin = p_util.getUrlQueryPara('uin');
console.log(uin);

var curCpId = p_pkg.getCpId();
var curGameId = p_pkg.getGameId();
console.log(curCpId);
console.log(curCpId);

var url_profile_g = p_util.getUrl("ad/j_profile.g", g_refresh_interval);



var file = p_util.getUrlQueryPara('file');
file += "?" + new Date().getTime();
console.log(file);

//初始化loading cirular
var g_lc = new LoadingCircular(40, [51, 73, 95]);
g_lc.draw();
g_lc.rotate();

// ready
$(document).ready(function() {
	
	$("#w-img").attr("src", file);
	
	
	
	$.getJSON(url_profile_g, {uin: uin}, function(jsonObj, stat) {
		
		var faceUrl = p_util.getAvatarUrl(jsonObj.value.userProfile.avtarImage);
		
		console.log();
		$("#w-face").attr("src", faceUrl);
		$("#w-nickname").html(jsonObj.value.userProfile.nickName);
		
	});
	
	
	
	
	$("#w-form").submit(function(evtO) {
		evtO.preventDefault();
		msg = $("#w-msg").val();
		p_pkg.syncMicroblogging(msg);
		
		if (p_pkg.isFromIPhoneClient()) {
			//alert("begin sync");
			p_util.invokeIPhoneFunc("syncMicroblogging", msg, "wdw");
		}
		
	});
	
}); // $$$ $(document).ready
