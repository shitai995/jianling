#if DEVELOPMENT_BUILD && !UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        private string playerReport;
        private float playerStarted;
        private int playerPhase, playerPages, playerTutorialPages, playerShopTutorialSteps;
        [Serializable] private sealed class PlayerReport
        {
            public bool passed;
            public string utc, unity, platform, error;
            public int dialoguePages;
            public string[] checks;
        }
        private void BeginPlayerValidation()
        {
            var args=Environment.GetCommandLineArgs();
            int flag=Array.IndexOf(args,"--gujian-smoke");
            if(flag<0 || flag+1>=args.Length)return;
            playerReport=Path.GetFullPath(args[flag+1]);
            playerStarted=Time.realtimeSinceStartup;
            saveFileName="validation-player.json";
            if(!originalUi || !originalUi.health || !originalUi.dialogue || originalUi.menu == null || originalUi.menu.actions.Length != 35 || originalUi.menuSheets.Length != 8 || !touchLayout || touchLayout.direction == null || DialoguePortrait) { FinishPlayerValidation("Saved UI references incomplete"); return; }
            Time.timeScale=8; Begin();
            Debug.Log("GUJIAN_PLAYER_SMOKE_STARTED");
        }
        private void TickPlayerValidation()
        {
            if(playerReport==null)return;
            try
            {
                if(LastError!=null)throw new Exception(LastError);
                if(Time.realtimeSinceStartup-playerStarted>120)throw new Exception("Player timed out in phase "+playerPhase);
                if(WeaponPreviewVisible){CloseWeaponPreview();return;}
                if(DialogueVisible){ContinueDialogue();playerPages++;return;}
                if(TutorialKey>=0)
                {
                    if(playerPhase==5)
                    {
                        if(TutorialKey==0)
                        {
                            RespondTutorial(0);playerShopTutorialSteps++;
                            int expected=playerShopTutorialSteps==1?23:24-playerShopTutorialSteps;
                            if(ShopSelectedItemId!=expected)throw new Exception("Player shop medicine selection mismatch");
                        }
                        else if(TutorialKey!=6||!CloseMenu())throw new Exception("Player shop return rejected");
                    }
                    else if(playerPhase==4)
                    {
                        if(TutorialKey==3)
                        {
                            if(SelectMenuPage(4)||CloseMenu())throw new Exception("Tutorial allowed wrong tab/early close");
                            int expected=new[]{2,1,6}[playerTutorialPages];
                            if(playerTutorialPages==1){if(!SelectMenuPage(expected))throw new Exception("Tutorial tab rejected");}
                            else RespondTutorial(3);
                            if(menuPage!=expected)throw new Exception("Tutorial page mismatch");
                            playerTutorialPages++;
                        }
                        else if(TutorialKey!=6||!CloseMenu())throw new Exception("Tutorial return rejected");
                    }
                    else RespondTutorial(TutorialKey);
                    return;
                }
                if(Machine.Running)return;
                if(playerPhase==0)
                {
                    if(Machine.Get("event")!=200 || Machine.Get("stageID")!=200)throw new Exception("Opening did not reach 200");
                    Machine.Enqueue("200n0");playerPhase=1;return;
                }
                if(playerPhase==1)
                {
                    if(Machine.Get("event")==201)
                    {
                        money=12345;durability=17;Role.hp=1234;Machine.Command(0x51,new object[]{0,3});
                        SetMission("主线存档验证","保留任务奖励",new[]{123,456,0},false);
                        SetMission("支线存档验证","保留剩余目标",new[]{789,1000,4},true);
                        tasks.Add(new PendingTask{stages=new[]{200},enemyIds=new[]{207,210},counts=new[]{7,2},script="taskover.s"});
                        Machine.Command(0x56,new object[0]);
                        Save();money=1;durability=1;Role.hp=1;Load();playerPhase=2;return;
                    }
                    var enemies=actors.Values.Where(a=>a.IsEnemy&&!a.hidden&&a.hp>0).ToArray();
                    foreach(var enemy in enemies)enemy.frozen=false;
                    if(enemies.Length>0 && Role.LogicalAction<=1)
                    {
                        var enemy=enemies[0];enemy.hp=1;
                        Role.mapPosition=enemy.mapPosition+new Vector2(-40,enemy.Animation.footHeight-Role.Animation.footHeight);
                        Role.Face("r");UseSkill(0,1,true);
                    }
                    return;
                }
                if(playerPhase==3)
                {
                    if(Machine.Get("stageID")!=200 || menu || worldMapOpen || !openAreas[10] || openAreas[9])throw new Exception("Player map travel mismatch");
                    Machine.Enqueue("guide4");playerPhase=4;return;
                }
                if(playerPhase==4)
                {
                    if(playerTutorialPages!=3||menu||scriptedMenu||Machine.Get("sub2")!=0)throw new Exception("Player spirit tutorial did not complete");
                    Machine.Command(0x67,new object[]{0});
                    if(!menu||!scriptedMenu)throw new Exception("Scripted menu open failed");
                    Machine.Command(0x67,new object[]{2});
                    if(menu||scriptedMenu)throw new Exception("Scripted menu close failed");
                    Machine.Enqueue("guide3");playerPhase=5;return;
                }
                if(playerPhase==5)
                {
                    if(playerShopTutorialSteps!=8||menu||scriptedMenu||Machine.Get("sub1")!=1)throw new Exception("Player shop tutorial did not finish");
                    Machine.Command(0x83,new object[]{5,"guide4.s"});TickCombat(.25f);Machine.Command(0x84,new object[0]);
                    spiritDurabilityCounter=7;spiritElapsed=.037f;
                    Save();Load();playerPhase=6;return;
                }
                if(playerPhase==6)
                {
                    if(timerScript!="guide4.s"||Mathf.Abs(timerSeconds-4.75f)>.0001f||!timerPaused||spiritDurabilityCounter!=7||Mathf.Abs(spiritElapsed-.037f)>.0001f)throw new Exception("Player fractional timer/cadence restore mismatch");
                    FinishPlayerValidation(null);return;
                }
                if(money!=12345 || durability!=17 || CurrentHp!=1234 || ItemCount(0)!=3 || Machine.Get("event")!=201)throw new Exception("Player save/load mismatch");
                if(MissionTitle!="主线存档验证" || BranchMissionTitle!="支线存档验证" || !mainMissionRewards.SequenceEqual(new[]{123,456,0}) || !branchMissionRewards.SequenceEqual(new[]{789,1000,4}) || tasks.Count!=1 || !tasks[0].counts.SequenceEqual(new[]{7,2}) || MissionObjectiveLabels().Length!=2)throw new Exception("Player mission save/load mismatch");
                if(Role.WeaponVisible)throw new Exception("Player lost saved stow state");
                int savedEnergy=Energy, savedDurability=durability, savedSkill=equippedSkill;
                int savedWeapon=equipmentId;
                int savedAction=Role.ActionVersion;
                Attack();
                if(UseSkill(4) || UseSkill(5) || TryCriticalKill() || Role.ActionVersion!=savedAction || Energy!=savedEnergy || durability!=savedDurability)throw new Exception("Stowed player attack/resource protection failed");
                Machine.Command(0x55,new object[]{-1});
                if(!Role.WeaponVisible || equippedSkill!=savedSkill || equipmentId!=savedWeapon)throw new Exception("Player drawing changed equipped weapon or skill");
                if(!openAreas[10] || TravelToArea(9))throw new Exception("Player visited/closed area persistence mismatch");
                Machine.Command(0x48,new object[]{10});
                if(Machine.Get("stageID")!=1 || !menu || !worldMapOpen || !TravelToArea(10))throw new Exception("Player map entry marker/selection mismatch");
                playerPhase=3;
            }
            catch(Exception ex){FinishPlayerValidation(ex.ToString());}
        }
        private void FinishPlayerValidation(string error)
        {
            var report=new PlayerReport{passed=error==null,utc=DateTime.UtcNow.ToString("O"),unity=Application.unityVersion,platform=Application.platform.ToString(),error=error,dialoguePages=playerPages,
                checks=new[]{"Saved UI asset, all 35 native menu actions/eight sheets and original touch direction/texture references loaded in standalone player","Original start/opening scripts -> stage 200 under real player Update","200n0 -> original attack rectangles, enemy respawns and four-kill callback -> event 201 (fixture enemy HP 1)","Separate player save: money 12345, HP 1234, durability 17, three items and event 201 restored","Main/branch mission titles, three reward slots (including equipment ID 0) and two remaining kill objectives restore in standalone player; setting reward metadata grants nothing","Saved stow state restored; player attacks reject without energy/durability loss; drawing preserves current weapon and skill","Visited area remains open after save/load; closed area rejects; x48 sets world-map stageID 1 and UI travel returns to stage 200 under actual player Update","Original guide4 completes with menu open, keyboard/tab page changes, wrong tab and early close rejection, return and x67 open/close in standalone Update","Original guide3 shop opens without blocking the VM, eight actual medicine selections 23 to16, return and sub1 completion under standalone Update","Real standalone Save/Load preserves paused fractional script timer and sword-spirit recovery cadence"}};
            File.WriteAllText(playerReport,JsonUtility.ToJson(report,true));
            Debug.Log("GUJIAN_PLAYER_SMOKE_FINISHED "+report.passed+" "+error);
            playerReport=null;Application.Quit(error==null?0:1);
        }
    }
}
#endif
