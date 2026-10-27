var updatePage = [ "ad/j_player.g", "ad/j_mailList.g",  "ad/j_gameDetail.g",  "ad/j_userCompare.g", "ad/j_scoreTypes.g", "ad/j_scoreRank.g",  "ad/j_achTypes.g", "ad/j_achRank.g", 'ad/j_profile.g'];



if (p_pkg.isFromAndroidClient) {
	
	if ( p_pkg.getData("isUpdate") == "true") { 
		for(var i = 0, p; p = updatePage[i]; i++) {
			p_pkg.setData(p, 51);
		}
		p_pkg.setData("isUpdate", "false");
	}
}