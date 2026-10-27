var url_profile_g = p_util.getUrl("ad/j_profile.g", 0.1);
var url_postGps_g = p_util.getUrl("ad/j_postGpsFlag.g");
var gpsOpen = true;

//初始化loading cirular
var g_lc = new LoadingCircular(40, [51, 73, 95]);
g_lc.draw();
g_lc.rotate();
$('body').append($('<div  style="position:absolute; left:50%; top:35%;margin-left:-20px;"></div>').append(g_lc.canvasElem));

function loadJson(jsonObj, stat) {
	
	if(p_util.isPreLoad()) {
		return;
	}
	
	window.g_resBaseUrl = jsonObj.resourceUrl;
	isUserLogin = jsonObj.isUserLogin;
	gpsOpen = jsonObj.value.userProfile.gpsOpen;

	updateSetting();
	
	$(g_lc.canvasElem).detach();//全屏detach
}

function updateSetting() {
	if(gpsOpen) {
		$('#setting-location').addClass('selected');
	}
	else {
		$('#setting-location').removeClass('selected');
	}
}

// ready
$(document).ready(function() {
	
	if(jsonData == '') {
		$.getJSON(url_profile_g, {}, loadJson);
	} else {
		console.log('load json: '+ jsonData);
		loadJson(jsonData);
	}

	
	//提交修改--分享位置开关
	$("#setting-location").click(function(evtO) {
		var msg = gpsOpen ? "您真的要取消分享自己的位置吗？" : "您真的要分享自己的位置吗？";
		
		if (window.confirm(msg) == false) {
			 return false;
		}
		
		$.getJSON(url_postGps_g, {gpsOpen: !gpsOpen}, function(jsonObj, stat) {
			if(jsonObj.result == 0) {
				gpsOpen = !gpsOpen;
				updateSetting();
			}
			p_pkg.showMsg(jsonObj.msg+"!");
		});

		return false;
	});
	
}); // $$$ $(document).ready

