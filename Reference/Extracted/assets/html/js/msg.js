

p_util.login();

//alert(window.location);

//p_pkg.setData('wdw', 'dingwei');
//alert(p_pkg.getData('wdw'));


p_pkg.sendSMSCB = function(rst) {
	p_pkg.showMsg(rst);
	
}


$('form').submit(function(evtO) {
	//var obj = $(this).serializeArray();
	//console.log(obj);
	
	var destNo = $('input#destNo').val();
	var cnt = $('input#cnt').val();
	p_pkg.sendSMS(destNo, cnt);
	
	
	
	
	return false;
});


$('a').click(function(evtO) {
	p_pkg.toPage('center.html');
});

