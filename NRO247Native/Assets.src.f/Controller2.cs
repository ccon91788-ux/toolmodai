using System;
using Assets.src.g;

namespace Assets.src.f;

internal class Controller2
{
	public static void readMessage(Message msg)
	{
		try
		{
			switch (msg.command)
			{
			case sbyte.MinValue:
				readInfoEffChar(msg);
				break;
			case sbyte.MaxValue:
				readInfoRada(msg);
				break;
			case 114:
				try
				{
					string text3 = msg.reader().readUTF();
					mSystem.curINAPP = msg.reader().readByte();
					mSystem.maxINAPP = msg.reader().readByte();
					break;
				}
				catch (Exception)
				{
					break;
				}
			case 113:
			{
				int loop = 0;
				int layer = 0;
				int id = 0;
				short x = 0;
				short y = 0;
				short loopCount = -1;
				try
				{
					loop = msg.reader().readByte();
					layer = msg.reader().readByte();
					id = msg.reader().readShort();
					x = msg.reader().readShort();
					y = msg.reader().readShort();
					loopCount = msg.reader().readShort();
				}
				catch (Exception)
				{
				}
				EffecMn.addEff(new Effect(id, x, y, layer, loop, loopCount));
				break;
			}
			case 48:
			{
				sbyte b2 = msg.reader().readByte();
				ServerListScreen.SetIpSelect(b2, issave: false);
				GameCanvas.instance.doResetToLoginScr(GameCanvas.serverScreen);
				Session_ME.gI().close();
				GameCanvas.endDlg();
				ServerListScreen.waitToLogin = true;
				break;
			}
			case 31:
			{
				int num9 = msg.reader().readInt();
				sbyte b8 = msg.reader().readByte();
				if (b8 == 1)
				{
					short smallID = msg.reader().readShort();
					sbyte b9 = -1;
					int[] array = null;
					short wimg = 0;
					short himg = 0;
					try
					{
						b9 = msg.reader().readByte();
						if (b9 > 0)
						{
							sbyte b10 = msg.reader().readByte();
							array = new int[b10];
							for (int num10 = 0; num10 < b10; num10++)
							{
								array[num10] = msg.reader().readByte();
							}
							wimg = msg.reader().readShort();
							himg = msg.reader().readShort();
						}
					}
					catch (Exception)
					{
					}
					if (num9 == Char.myCharz().charID)
					{
						Char.myCharz().petFollow = new PetFollow();
						Char.myCharz().petFollow.smallID = smallID;
						if (b9 > 0)
						{
							Char.myCharz().petFollow.SetImg(b9, array, wimg, himg);
						}
						break;
					}
					Char char3 = GameScr.findCharInMap(num9);
					char3.petFollow = new PetFollow();
					char3.petFollow.smallID = smallID;
					if (b9 > 0)
					{
						char3.petFollow.SetImg(b9, array, wimg, himg);
					}
				}
				else if (num9 == Char.myCharz().charID)
				{
					Char.myCharz().petFollow.remove();
					Char.myCharz().petFollow = null;
				}
				else
				{
					Char char4 = GameScr.findCharInMap(num9);
					char4.petFollow.remove();
					char4.petFollow = null;
				}
				break;
			}
			case -89:
				GameCanvas.open3Hour = msg.reader().readByte() == 1;
				break;
			case 42:
			{
				GameCanvas.endDlg();
				LoginScr.isContinueToLogin = false;
				Char.isLoadingMap = false;
				sbyte haveName = msg.reader().readByte();
				if (GameCanvas.registerScr == null)
				{
					GameCanvas.registerScr = new RegisterScreen(haveName);
				}
				GameCanvas.registerScr.switchToMe();
				break;
			}
			case 52:
			{
				sbyte b15 = msg.reader().readByte();
				if (b15 == 1)
				{
					int num18 = msg.reader().readInt();
					if (num18 == Char.myCharz().charID)
					{
						Char.myCharz().setMabuHold(m: true);
						Char.myCharz().cx = msg.reader().readShort();
						Char.myCharz().cy = msg.reader().readShort();
					}
					else
					{
						Char char5 = GameScr.findCharInMap(num18);
						if (char5 != null)
						{
							char5.setMabuHold(m: true);
							char5.cx = msg.reader().readShort();
							char5.cy = msg.reader().readShort();
						}
					}
				}
				if (b15 == 0)
				{
					int num19 = msg.reader().readInt();
					if (num19 == Char.myCharz().charID)
					{
						Char.myCharz().setMabuHold(m: false);
					}
					else
					{
						GameScr.findCharInMap(num19)?.setMabuHold(m: false);
					}
				}
				if (b15 == 2)
				{
					int charId2 = msg.reader().readInt();
					int id3 = msg.reader().readInt();
					Mabu mabu2 = (Mabu)GameScr.findCharInMap(charId2);
					mabu2.eat(id3);
				}
				if (b15 == 3)
				{
					GameScr.mabuPercent = msg.reader().readByte();
				}
				break;
			}
			case 51:
			{
				int charId = msg.reader().readInt();
				Mabu mabu = (Mabu)GameScr.findCharInMap(charId);
				sbyte id2 = msg.reader().readByte();
				short x2 = msg.reader().readShort();
				short y2 = msg.reader().readShort();
				sbyte b12 = msg.reader().readByte();
				Char[] array4 = new Char[b12];
				long[] array5 = new long[b12];
				for (int num11 = 0; num11 < b12; num11++)
				{
					int num13 = msg.reader().readInt();
					Res.outz("char ID=" + num13);
					array4[num11] = null;
					if (num13 != Char.myCharz().charID)
					{
						array4[num11] = GameScr.findCharInMap(num13);
					}
					else
					{
						array4[num11] = Char.myCharz();
					}
					array5[num11] = msg.reader().readLong();
				}
				mabu.setSkill(id2, x2, y2, array4, array5);
				break;
			}
			case -127:
				readLuckyRound(msg);
				break;
			case -126:
			{
				sbyte b21 = msg.reader().readByte();
				Res.outz("type quay= " + b21);
				if (b21 == 1)
				{
					sbyte b23 = msg.reader().readByte();
					string num35 = msg.reader().readUTF();
					string finish = msg.reader().readUTF();
					GameScr.gI().showWinNumber(num35, finish);
				}
				if (b21 == 0)
				{
					GameScr.gI().showYourNumber(msg.reader().readUTF());
				}
				break;
			}
			case -122:
			{
				short id4 = msg.reader().readShort();
				Npc npc = GameScr.findNPCInMap(id4);
				sbyte b20 = msg.reader().readByte();
				npc.duahau = new int[b20];
				Res.outz("N DUA HAU= " + b20);
				for (int num33 = 0; num33 < b20; num33++)
				{
					npc.duahau[num33] = msg.reader().readShort();
				}
				npc.setStatus(msg.reader().readByte(), msg.reader().readInt());
				break;
			}
			case 102:
			{
				sbyte b16 = msg.reader().readByte();
				if (b16 == 0 || b16 == 1 || b16 == 2 || b16 == 6)
				{
					BigBoss2 bigBoss2 = Mob.getBigBoss2();
					if (bigBoss2 == null)
					{
						break;
					}
					if (b16 == 6)
					{
						bigBoss2.x = (bigBoss2.y = (bigBoss2.xTo = (bigBoss2.yTo = (bigBoss2.xFirst = (bigBoss2.yFirst = -1000)))));
						break;
					}
					sbyte b17 = msg.reader().readByte();
					Char[] array9 = new Char[b17];
					long[] array10 = new long[b17];
					for (int num26 = 0; num26 < b17; num26++)
					{
						int num27 = msg.reader().readInt();
						array9[num26] = null;
						if (num27 != Char.myCharz().charID)
						{
							array9[num26] = GameScr.findCharInMap(num27);
						}
						else
						{
							array9[num26] = Char.myCharz();
						}
						array10[num26] = msg.reader().readLong();
					}
					bigBoss2.setAttack(array9, array10, b16);
				}
				if (b16 == 3 || b16 == 4 || b16 == 5 || b16 == 7)
				{
					BachTuoc bachTuoc = Mob.getBachTuoc();
					if (bachTuoc == null)
					{
						break;
					}
					if (b16 == 7)
					{
						bachTuoc.x = (bachTuoc.y = (bachTuoc.xTo = (bachTuoc.yTo = (bachTuoc.xFirst = (bachTuoc.yFirst = -1000)))));
						break;
					}
					if (b16 == 3 || b16 == 4)
					{
						sbyte b18 = msg.reader().readByte();
						Char[] array11 = new Char[b18];
						long[] array2 = new long[b18];
						for (int num28 = 0; num28 < b18; num28++)
						{
							int num29 = msg.reader().readInt();
							array11[num28] = null;
							if (num29 != Char.myCharz().charID)
							{
								array11[num28] = GameScr.findCharInMap(num29);
							}
							else
							{
								array11[num28] = Char.myCharz();
							}
							array2[num28] = msg.reader().readLong();
						}
						bachTuoc.setAttack(array11, array2, b16);
					}
					if (b16 == 5)
					{
						short xMoveTo = msg.reader().readShort();
						bachTuoc.move(xMoveTo);
					}
				}
				if (b16 > 9 && b16 < 30)
				{
					readActionBoss(msg, b16);
				}
				break;
			}
			case 101:
			{
				Res.outz("big boss--------------------------------------------------");
				BigBoss bigBoss = Mob.getBigBoss();
				if (bigBoss == null)
				{
					break;
				}
				sbyte b13 = msg.reader().readByte();
				if (b13 == 0 || b13 == 1 || b13 == 2 || b13 == 4 || b13 == 3)
				{
					if (b13 == 3)
					{
						bigBoss.xTo = (bigBoss.xFirst = msg.reader().readShort());
						bigBoss.yTo = (bigBoss.yFirst = msg.reader().readShort());
						bigBoss.setFly();
					}
					else
					{
						sbyte b14 = msg.reader().readByte();
						Res.outz("CHUONG nChar= " + b14);
						Char[] array6 = new Char[b14];
						long[] array7 = new long[b14];
						for (int num14 = 0; num14 < b14; num14++)
						{
							int num15 = msg.reader().readInt();
							Res.outz("char ID=" + num15);
							array6[num14] = null;
							if (num15 != Char.myCharz().charID)
							{
								array6[num14] = GameScr.findCharInMap(num15);
							}
							else
							{
								array6[num14] = Char.myCharz();
							}
							array7[num14] = msg.reader().readLong();
						}
						bigBoss.setAttack(array6, array7, b13);
					}
				}
				if (b13 == 5)
				{
					bigBoss.haftBody = true;
					bigBoss.status = 2;
				}
				if (b13 == 6)
				{
					bigBoss.getDataB2();
					bigBoss.x = msg.reader().readShort();
					bigBoss.y = msg.reader().readShort();
				}
				if (b13 == 7)
				{
					bigBoss.setAttack(null, null, b13);
				}
				if (b13 == 8)
				{
					bigBoss.xTo = (bigBoss.xFirst = msg.reader().readShort());
					bigBoss.yTo = (bigBoss.yFirst = msg.reader().readShort());
					bigBoss.status = 2;
				}
				if (b13 == 9)
				{
					bigBoss.x = (bigBoss.y = (bigBoss.xTo = (bigBoss.yTo = (bigBoss.xFirst = (bigBoss.yFirst = -1000)))));
				}
				break;
			}
			case -120:
			{
				long num17 = mSystem.currentTimeMillis();
				Service.logController = num17 - Service.curCheckController;
				Service.gI().sendCheckController();
				break;
			}
			case -121:
			{
				long num20 = mSystem.currentTimeMillis();
				Service.logMap = num20 - Service.curCheckMap;
				Service.gI().sendCheckMap();
				break;
			}
			case 100:
			{
				sbyte b24 = msg.reader().readByte();
				sbyte b25 = msg.reader().readByte();
				Item item2 = null;
				if (b24 == 0)
				{
					item2 = Char.myCharz().arrItemBody[b25];
				}
				if (b24 == 1)
				{
					item2 = Char.myCharz().arrItemBag[b25];
				}
				short num36 = msg.reader().readShort();
				if (num36 == -1)
				{
					break;
				}
				item2.template = ItemTemplates.get(num36);
				item2.quantity = msg.reader().readInt();
				item2.info = msg.reader().readUTF();
				item2.content = msg.reader().readUTF();
				sbyte b26 = msg.reader().readByte();
				if (b26 != 0)
				{
					item2.itemOption = new ItemOption[b26];
					for (int num37 = 0; num37 < item2.itemOption.Length; num37++)
					{
						ItemOption itemOption3 = Controller.gI().readItemOption(msg);
						if (itemOption3 != null)
						{
							item2.itemOption[num37] = itemOption3;
						}
					}
				}
				if (item2.quantity <= 0)
				{
					item2 = null;
				}
				break;
			}
			case -123:
			{
				int charId3 = msg.reader().readInt();
				if (GameScr.findCharInMap(charId3) != null)
				{
					GameScr.findCharInMap(charId3).perCentMp = msg.reader().readByte();
				}
				break;
			}
			case -119:
				Char.myCharz().rank = msg.reader().readInt();
				break;
			case -117:
				GameScr.gI().tMabuEff = 0;
				GameScr.gI().percentMabu = msg.reader().readByte();
				if (GameScr.gI().percentMabu == 100)
				{
					GameScr.gI().mabuEff = true;
				}
				if (GameScr.gI().percentMabu == 101)
				{
					Npc.mabuEff = true;
				}
				break;
			case -116:
				GameScr.canAutoPlay = msg.reader().readByte() == 1;
				break;
			case -115:
				Char.myCharz().setPowerInfo(msg.reader().readUTF(), msg.reader().readShort(), msg.reader().readShort(), msg.reader().readShort());
				break;
			case -113:
			{
				sbyte[] array8 = new sbyte[10];
				for (int num22 = 0; num22 < 10; num22++)
				{
					array8[num22] = msg.reader().readByte();
					Res.outz("vlue i= " + array8[num22]);
				}
				GameScr.gI().onKSkill(array8);
				GameScr.gI().onOSkill(array8);
				GameScr.gI().onCSkill(array8);
				break;
			}
			case -111:
			{
				short num2 = msg.reader().readShort();
				ImageSource.vSource = new MyVector();
				for (int l = 0; l < num2; l++)
				{
					string iD = msg.reader().readUTF();
					sbyte version = msg.reader().readByte();
					ImageSource.vSource.addElement(new ImageSource(iD, version));
				}
				ImageSource.checkRMS();
				ImageSource.saveRMS();
				break;
			}
			case 125:
			{
				sbyte fusion = msg.reader().readByte();
				int num3 = msg.reader().readInt();
				if (num3 == Char.myCharz().charID)
				{
					Char.myCharz().setFusion(fusion);
				}
				else if (GameScr.findCharInMap(num3) != null)
				{
					GameScr.findCharInMap(num3).setFusion(fusion);
				}
				break;
			}
			case 124:
			{
				short num16 = msg.reader().readShort();
				string text4 = msg.reader().readUTF();
				Res.outz("noi chuyen = " + text4 + "npc ID= " + num16);
				GameScr.findNPCInMap(num16)?.addInfo(text4);
				break;
			}
			case 123:
			{
				Res.outz("SET POSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSss");
				int num23 = msg.reader().readInt();
				short xPos = msg.reader().readShort();
				short yPos = msg.reader().readShort();
				sbyte b27 = msg.reader().readByte();
				Char @char = null;
				if (num23 == Char.myCharz().charID)
				{
					@char = Char.myCharz();
				}
				else if (GameScr.findCharInMap(num23) != null)
				{
					@char = GameScr.findCharInMap(num23);
				}
				if (@char != null)
				{
					ServerEffect.addServerEffect((b27 != 0) ? 173 : 60, @char, 1);
					@char.setPos(xPos, yPos, b27);
				}
				break;
			}
			case 122:
			{
				short num21 = msg.reader().readShort();
				Res.outz("second login = " + num21);
				LoginScr.timeLogin = num21;
				LoginScr.currTimeLogin = (LoginScr.lastTimeLogin = mSystem.currentTimeMillis());
				GameCanvas.endDlg();
				break;
			}
			case 121:
				mSystem.publicID = msg.reader().readUTF();
				mSystem.strAdmob = msg.reader().readUTF();
				Res.outz("SHOW AD public ID= " + mSystem.publicID);
				mSystem.createAdmob();
				break;
			case -124:
			{
				sbyte b30 = msg.reader().readByte();
				sbyte b31 = msg.reader().readByte();
				if (b31 == 0)
				{
					if (b30 == 2)
					{
						int num34 = msg.reader().readInt();
						if (num34 == Char.myCharz().charID)
						{
							Char.myCharz().removeEffect();
						}
						else if (GameScr.findCharInMap(num34) != null)
						{
							GameScr.findCharInMap(num34).removeEffect();
						}
					}
					int num38 = msg.reader().readUnsignedByte();
					int num39 = msg.reader().readInt();
					if (num38 == 32)
					{
						if (b30 == 1)
						{
							int num40 = msg.reader().readInt();
							if (num39 == Char.myCharz().charID)
							{
								Char.myCharz().holdEffID = num38;
								GameScr.findCharInMap(num40).setHoldChar(Char.myCharz());
							}
							else if (GameScr.findCharInMap(num39) != null && num40 != Char.myCharz().charID)
							{
								GameScr.findCharInMap(num39).holdEffID = num38;
								GameScr.findCharInMap(num40).setHoldChar(GameScr.findCharInMap(num39));
							}
							else if (GameScr.findCharInMap(num39) != null && num40 == Char.myCharz().charID)
							{
								GameScr.findCharInMap(num39).holdEffID = num38;
								Char.myCharz().setHoldChar(GameScr.findCharInMap(num39));
							}
						}
						else if (num39 == Char.myCharz().charID)
						{
							Char.myCharz().removeHoleEff();
						}
						else if (GameScr.findCharInMap(num39) != null)
						{
							GameScr.findCharInMap(num39).removeHoleEff();
						}
					}
					if (num38 == 33)
					{
						if (b30 == 1)
						{
							if (num39 == Char.myCharz().charID)
							{
								Char.myCharz().protectEff = true;
							}
							else if (GameScr.findCharInMap(num39) != null)
							{
								GameScr.findCharInMap(num39).protectEff = true;
							}
						}
						else if (num39 == Char.myCharz().charID)
						{
							Char.myCharz().removeProtectEff();
						}
						else if (GameScr.findCharInMap(num39) != null)
						{
							GameScr.findCharInMap(num39).removeProtectEff();
						}
					}
					if (num38 == 39)
					{
						if (b30 == 1)
						{
							if (num39 == Char.myCharz().charID)
							{
								Char.myCharz().huytSao = true;
							}
							else if (GameScr.findCharInMap(num39) != null)
							{
								GameScr.findCharInMap(num39).huytSao = true;
							}
						}
						else if (num39 == Char.myCharz().charID)
						{
							Char.myCharz().removeHuytSao();
						}
						else if (GameScr.findCharInMap(num39) != null)
						{
							GameScr.findCharInMap(num39).removeHuytSao();
						}
					}
					if (num38 == 40)
					{
						if (b30 == 1)
						{
							if (num39 == Char.myCharz().charID)
							{
								Char.myCharz().blindEff = true;
							}
							else if (GameScr.findCharInMap(num39) != null)
							{
								GameScr.findCharInMap(num39).blindEff = true;
							}
						}
						else if (num39 == Char.myCharz().charID)
						{
							Char.myCharz().removeBlindEff();
						}
						else if (GameScr.findCharInMap(num39) != null)
						{
							GameScr.findCharInMap(num39).removeBlindEff();
						}
					}
					if (num38 == 41)
					{
						if (b30 == 1)
						{
							if (num39 == Char.myCharz().charID)
							{
								Char.myCharz().sleepEff = true;
							}
							else if (GameScr.findCharInMap(num39) != null)
							{
								GameScr.findCharInMap(num39).sleepEff = true;
							}
						}
						else if (num39 == Char.myCharz().charID)
						{
							Char.myCharz().removeSleepEff();
						}
						else if (GameScr.findCharInMap(num39) != null)
						{
							GameScr.findCharInMap(num39).removeSleepEff();
						}
					}
					if (num38 == 42)
					{
						if (b30 == 1)
						{
							if (num39 == Char.myCharz().charID)
							{
								Char.myCharz().stone = true;
							}
						}
						else if (num39 == Char.myCharz().charID)
						{
							Char.myCharz().stone = false;
						}
					}
				}
				if (b31 != 1)
				{
					break;
				}
				int num41 = msg.reader().readUnsignedByte();
				sbyte b32 = msg.reader().readByte();
				Res.outz("modbHoldID= " + b32 + " skillID= " + num41 + "eff ID= " + b30);
				if (num41 == 32)
				{
					if (b30 == 1)
					{
						int num42 = msg.reader().readInt();
						if (num42 == Char.myCharz().charID)
						{
							GameScr.findMobInMap(b32).holdEffID = num41;
							Char.myCharz().setHoldMob(GameScr.findMobInMap(b32));
						}
						else if (GameScr.findCharInMap(num42) != null)
						{
							GameScr.findMobInMap(b32).holdEffID = num41;
							GameScr.findCharInMap(num42).setHoldMob(GameScr.findMobInMap(b32));
						}
					}
					else
					{
						GameScr.findMobInMap(b32).removeHoldEff();
					}
				}
				if (num41 == 40)
				{
					if (b30 == 1)
					{
						GameScr.findMobInMap(b32).blindEff = true;
					}
					else
					{
						GameScr.findMobInMap(b32).removeBlindEff();
					}
				}
				if (num41 == 41)
				{
					if (b30 == 1)
					{
						GameScr.findMobInMap(b32).sleepEff = true;
					}
					else
					{
						GameScr.findMobInMap(b32).removeSleepEff();
					}
				}
				break;
			}
			case -125:
			{
				ChatTextField.gI().isShow = false;
				string text = msg.reader().readUTF();
				Res.outz("titile= " + text);
				sbyte b28 = msg.reader().readByte();
				ClientInput.gI().setInput(b28, text);
				for (int k = 0; k < b28; k++)
				{
					ClientInput.gI().tf[k].name = msg.reader().readUTF();
					sbyte b29 = msg.reader().readByte();
					if (b29 == 0)
					{
						ClientInput.gI().tf[k].setIputType(TField.INPUT_TYPE_NUMERIC);
					}
					if (b29 == 1)
					{
						ClientInput.gI().tf[k].setIputType(TField.INPUT_TYPE_ANY);
					}
					if (b29 == 2)
					{
						ClientInput.gI().tf[k].setIputType(TField.INPUT_TYPE_PASSWORD);
					}
				}
				break;
			}
			case -110:
			{
				sbyte b19 = msg.reader().readByte();
				if (b19 == 1)
				{
					int num30 = msg.reader().readInt();
					sbyte[] array3 = Rms.loadRMS(num30 + string.Empty);
					if (array3 == null)
					{
						Service.gI().sendServerData(1, -1, null);
					}
					else
					{
						Service.gI().sendServerData(1, num30, array3);
					}
				}
				if (b19 == 0)
				{
					int num31 = msg.reader().readInt();
					short num32 = msg.reader().readShort();
					sbyte[] data = new sbyte[num32];
					msg.reader().read(ref data, 0, num32);
					Rms.saveRMS(num31 + string.Empty, data);
				}
				break;
			}
			case 93:
			{
				string str = msg.reader().readUTF();
				str = Res.changeString(str);
				if (!str.ToLower().Contains("trò chơi dành cho người chơi trên 18 tuổi"))
				{
					GameScr.gI().chatVip(str);
				}
				break;
			}
			case -106:
			{
				short num24 = msg.reader().readShort();
				int num25 = msg.reader().readShort();
				if (ItemTime.isExistItem(num24))
				{
					ItemTime.getItemById(num24).initTime(num25);
					break;
				}
				ItemTime o = new ItemTime(num24, num25);
				Char.vItemTime.addElement(o);
				break;
			}
			case -105:
				TransportScr.gI().time = 0;
				TransportScr.gI().maxTime = msg.reader().readShort();
				TransportScr.gI().last = (TransportScr.gI().curr = mSystem.currentTimeMillis());
				TransportScr.gI().type = msg.reader().readByte();
				TransportScr.gI().switchToMe();
				break;
			case -103:
				switch (msg.reader().readByte())
				{
				case 0:
				{
					GameCanvas.panel.vFlag.removeAllElements();
					sbyte b4 = msg.reader().readByte();
					for (int m = 0; m < b4; m++)
					{
						Item item = new Item();
						short num4 = msg.reader().readShort();
						if (num4 != -1)
						{
							item.template = ItemTemplates.get(num4);
							sbyte b5 = msg.reader().readByte();
							if (b5 != -1)
							{
								item.itemOption = new ItemOption[b5];
								for (int n = 0; n < item.itemOption.Length; n++)
								{
									ItemOption itemOption2 = Controller.gI().readItemOption(msg);
									if (itemOption2 != null)
									{
										item.itemOption[n] = itemOption2;
									}
								}
							}
						}
						GameCanvas.panel.vFlag.addElement(item);
					}
					GameCanvas.panel.setTypeFlag();
					GameCanvas.panel.show();
					break;
				}
				case 1:
				{
					int num5 = msg.reader().readInt();
					sbyte b6 = msg.reader().readByte();
					Res.outz("---------------actionFlag1:  " + num5 + " : " + b6);
					if (num5 == Char.myCharz().charID)
					{
						Char.myCharz().cFlag = b6;
					}
					else if (GameScr.findCharInMap(num5) != null)
					{
						GameScr.findCharInMap(num5).cFlag = b6;
					}
					GameScr.gI().getFlagImage(num5, b6);
					break;
				}
				case 2:
				{
					sbyte b7 = msg.reader().readByte();
					int num6 = msg.reader().readShort();
					PKFlag pKFlag = new PKFlag();
					pKFlag.cflag = b7;
					pKFlag.IDimageFlag = num6;
					GameScr.vFlag.addElement(pKFlag);
					for (int num7 = 0; num7 < GameScr.vFlag.size(); num7++)
					{
						PKFlag pKFlag2 = (PKFlag)GameScr.vFlag.elementAt(num7);
						Res.outz("i: " + num7 + "  cflag: " + pKFlag2.cflag + "   IDimageFlag: " + pKFlag2.IDimageFlag);
					}
					for (int num8 = 0; num8 < GameScr.vCharInMap.size(); num8++)
					{
						Char char2 = (Char)GameScr.vCharInMap.elementAt(num8);
						if (char2 != null && char2.cFlag == b7)
						{
							char2.flagImage = num6;
						}
					}
					if (Char.myCharz().cFlag == b7)
					{
						Char.myCharz().flagImage = num6;
					}
					break;
				}
				}
				break;
			case -102:
			{
				sbyte b3 = msg.reader().readByte();
				if (b3 != 0 && b3 == 1)
				{
					GameCanvas.loginScr.isLogin2 = false;
					Service.gI().login(Rms.loadRMSString("acc"), Rms.loadRMSString("pass"), GameMidlet.VERSION, 0);
					LoginScr.isLoggingIn = true;
				}
				break;
			}
			case -101:
			{
				GameCanvas.loginScr.isLogin2 = true;
				GameCanvas.connect();
				string text2 = msg.reader().readUTF();
				Rms.saveRMSString("userAo" + ServerListScreen.ipSelect, text2);
				Service.gI().setClientType();
				Service.gI().login(text2, string.Empty, GameMidlet.VERSION, 1);
				break;
			}
			case -100:
			{
				InfoDlg.hide();
				bool flag = false;
				if (GameCanvas.w > 2 * PanelG.WIDTH_PANEL)
				{
					flag = true;
				}
				sbyte b = msg.reader().readByte();
				if (b < 0)
				{
					break;
				}
				Res.outz("t Indxe= " + b);
				GameCanvas.panel.maxPageShop[b] = msg.reader().readByte();
				GameCanvas.panel.currPageShop[b] = msg.reader().readByte();
				Res.outz("max page= " + GameCanvas.panel.maxPageShop[b] + " curr page= " + GameCanvas.panel.currPageShop[b]);
				int num = msg.reader().readUnsignedByte();
				Char.myCharz().arrItemShop[b] = new Item[num];
				for (int i = 0; i < num; i++)
				{
					short num12 = msg.reader().readShort();
					if (num12 == -1)
					{
						continue;
					}
					Res.outz("template id= " + num12);
					Char.myCharz().arrItemShop[b][i] = new Item();
					Char.myCharz().arrItemShop[b][i].template = ItemTemplates.get(num12);
					Char.myCharz().arrItemShop[b][i].itemId = msg.reader().readShort();
					Char.myCharz().arrItemShop[b][i].buyCoin = msg.reader().readInt();
					Char.myCharz().arrItemShop[b][i].buyGold = msg.reader().readInt();
					Char.myCharz().arrItemShop[b][i].buyType = msg.reader().readByte();
					Char.myCharz().arrItemShop[b][i].quantity = msg.reader().readInt();
					Char.myCharz().arrItemShop[b][i].isMe = msg.reader().readByte();
					PanelG.strWantToBuy = mResources.say_wat_do_u_want_to_buy;
					sbyte b11 = msg.reader().readByte();
					if (b11 != -1)
					{
						Char.myCharz().arrItemShop[b][i].itemOption = new ItemOption[b11];
						for (int j = 0; j < Char.myCharz().arrItemShop[b][i].itemOption.Length; j++)
						{
							ItemOption itemOption = Controller.gI().readItemOption(msg);
							if (itemOption != null)
							{
								Char.myCharz().arrItemShop[b][i].itemOption[j] = itemOption;
								Char.myCharz().arrItemShop[b][i].compare = GameCanvas.panel.getCompare(Char.myCharz().arrItemShop[b][i]);
							}
						}
					}
					sbyte b22 = msg.reader().readByte();
					if (b22 == 1)
					{
						int headTemp = msg.reader().readShort();
						int bodyTemp = msg.reader().readShort();
						int legTemp = msg.reader().readShort();
						int bagTemp = msg.reader().readShort();
						Char.myCharz().arrItemShop[b][i].setPartTemp(headTemp, bodyTemp, legTemp, bagTemp);
					}
					if (GameMidlet.intVERSION >= 237)
					{
						Char.myCharz().arrItemShop[b][i].nameNguoiKyGui = msg.reader().readUTF();
						Res.err("nguoi ki gui  " + Char.myCharz().arrItemShop[b][i].nameNguoiKyGui);
					}
				}
				if (flag)
				{
					GameCanvas.panel2.setTabKiGui();
				}
				GameCanvas.panel.setTabShop();
				GameCanvas.panel.cmy = (GameCanvas.panel.cmtoY = 0);
				break;
			}
			}
		}
		catch (Exception ex4)
		{
			Res.outz("=====> Controller2 " + ex4.StackTrace);
		}
	}

	private static void readLuckyRound(Message msg)
	{
		try
		{
			switch (msg.reader().readByte())
			{
			case 0:
			{
				sbyte b2 = msg.reader().readByte();
				short[] array = new short[b2];
				for (int i = 0; i < b2; i++)
				{
					array[i] = msg.reader().readShort();
				}
				sbyte b3 = msg.reader().readByte();
				int price = msg.reader().readInt();
				short idTicket = msg.reader().readShort();
				CrackBallScr.gI().SetCrackBallScr(array, (byte)b3, price, idTicket);
				break;
			}
			case 1:
			{
				sbyte b4 = msg.reader().readByte();
				short[] array2 = new short[b4];
				for (int j = 0; j < b4; j++)
				{
					array2[j] = msg.reader().readShort();
				}
				CrackBallScr.gI().DoneCrackBallScr(array2);
				break;
			}
			}
		}
		catch (Exception)
		{
		}
	}

	private static void readInfoRada(Message msg)
	{
		try
		{
			switch (msg.reader().readByte())
			{
			case 0:
			{
				RadarScr.gI();
				MyVector myVector = new MyVector(string.Empty);
				short num = msg.reader().readShort();
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					Info_RadaScr info_RadaScr = new Info_RadaScr();
					int id = msg.reader().readShort();
					int no = i + 1;
					int idIcon = msg.reader().readShort();
					sbyte rank = msg.reader().readByte();
					sbyte amount = msg.reader().readByte();
					sbyte max_amount = msg.reader().readByte();
					short templateId = -1;
					Char charInfo = null;
					sbyte b2 = msg.reader().readByte();
					if (b2 == 0)
					{
						templateId = msg.reader().readShort();
					}
					else
					{
						int head = msg.reader().readShort();
						int body = msg.reader().readShort();
						int leg = msg.reader().readShort();
						int bag = msg.reader().readShort();
						charInfo = Info_RadaScr.SetCharInfo(head, body, leg, bag);
					}
					string name = msg.reader().readUTF();
					string info = msg.reader().readUTF();
					sbyte b3 = msg.reader().readByte();
					sbyte use = msg.reader().readByte();
					sbyte b4 = msg.reader().readByte();
					ItemOption[] array = null;
					if (b4 != 0)
					{
						array = new ItemOption[b4];
						for (int j = 0; j < array.Length; j++)
						{
							ItemOption itemOption = Controller.gI().readItemOption(msg);
							sbyte activeCard = msg.reader().readByte();
							if (itemOption != null)
							{
								array[j] = itemOption;
								array[j].activeCard = activeCard;
							}
						}
					}
					info_RadaScr.SetInfo(id, no, idIcon, rank, b2, templateId, name, info, charInfo, array);
					info_RadaScr.SetLevel(b3);
					info_RadaScr.SetUse(use);
					info_RadaScr.SetAmount(amount, max_amount);
					myVector.addElement(info_RadaScr);
					if (b3 > 0)
					{
						num2++;
					}
				}
				RadarScr.gI().SetRadarScr(myVector, num2, num);
				RadarScr.gI().switchToMe();
				break;
			}
			case 1:
			{
				int id2 = msg.reader().readShort();
				sbyte use2 = msg.reader().readByte();
				if (Info_RadaScr.GetInfo(RadarScr.list, id2) != null)
				{
					Info_RadaScr.GetInfo(RadarScr.list, id2).SetUse(use2);
				}
				RadarScr.SetListUse();
				break;
			}
			case 2:
			{
				int num3 = msg.reader().readShort();
				sbyte level = msg.reader().readByte();
				int num4 = 0;
				for (int k = 0; k < RadarScr.list.size(); k++)
				{
					Info_RadaScr info_RadaScr2 = (Info_RadaScr)RadarScr.list.elementAt(k);
					if (info_RadaScr2 != null)
					{
						if (info_RadaScr2.id == num3)
						{
							info_RadaScr2.SetLevel(level);
						}
						if (info_RadaScr2.level > 0)
						{
							num4++;
						}
					}
				}
				RadarScr.SetNum(num4, RadarScr.list.size());
				if (Info_RadaScr.GetInfo(RadarScr.listUse, num3) != null)
				{
					Info_RadaScr.GetInfo(RadarScr.listUse, num3).SetLevel(level);
				}
				break;
			}
			case 3:
			{
				int id3 = msg.reader().readShort();
				sbyte amount2 = msg.reader().readByte();
				sbyte max_amount2 = msg.reader().readByte();
				if (Info_RadaScr.GetInfo(RadarScr.list, id3) != null)
				{
					Info_RadaScr.GetInfo(RadarScr.list, id3).SetAmount(amount2, max_amount2);
				}
				if (Info_RadaScr.GetInfo(RadarScr.listUse, id3) != null)
				{
					Info_RadaScr.GetInfo(RadarScr.listUse, id3).SetAmount(amount2, max_amount2);
				}
				break;
			}
			case 4:
			{
				int num5 = msg.reader().readInt();
				short idAuraEff = msg.reader().readShort();
				Char @char = null;
				@char = ((num5 != Char.myCharz().charID) ? GameScr.findCharInMap(num5) : Char.myCharz());
				if (@char != null)
				{
					@char.idAuraEff = idAuraEff;
					@char.idEff_Set_Item = msg.reader().readByte();
				}
				break;
			}
			}
		}
		catch (Exception)
		{
		}
	}

	private static void readInfoEffChar(Message msg)
	{
		try
		{
			sbyte b = msg.reader().readByte();
			int num = msg.reader().readInt();
			Char @char = null;
			@char = ((num != Char.myCharz().charID) ? GameScr.findCharInMap(num) : Char.myCharz());
			switch (b)
			{
			case 0:
			{
				int id = msg.reader().readShort();
				int layer = msg.reader().readByte();
				int loop = msg.reader().readByte();
				short loopCount = msg.reader().readShort();
				sbyte isStand = msg.reader().readByte();
				@char?.addEffChar(new Effect(id, @char, layer, loop, loopCount, isStand));
				break;
			}
			case 1:
			{
				int id2 = msg.reader().readShort();
				@char?.removeEffChar(0, id2);
				break;
			}
			case 2:
				@char?.removeEffChar(-1, 0);
				break;
			}
		}
		catch (Exception)
		{
		}
	}

	private static void readActionBoss(Message msg, int actionBoss)
	{
		try
		{
			sbyte idBoss = msg.reader().readByte();
			NewBoss newBoss = Mob.getNewBoss(idBoss);
			if (newBoss == null)
			{
				return;
			}
			if (actionBoss == 10)
			{
				short xMoveTo = msg.reader().readShort();
				short yMoveTo = msg.reader().readShort();
				newBoss.move(xMoveTo, yMoveTo);
			}
			if (actionBoss >= 11 && actionBoss <= 20)
			{
				sbyte b = msg.reader().readByte();
				Char[] array = new Char[b];
				long[] array2 = new long[b];
				for (int i = 0; i < b; i++)
				{
					int num = msg.reader().readInt();
					array[i] = null;
					if (num != Char.myCharz().charID)
					{
						array[i] = GameScr.findCharInMap(num);
					}
					else
					{
						array[i] = Char.myCharz();
					}
					array2[i] = msg.reader().readLong();
				}
				sbyte dir = msg.reader().readByte();
				newBoss.setAttack(array, array2, (sbyte)(actionBoss - 10), dir);
			}
			if (actionBoss == 21)
			{
				newBoss.xTo = msg.reader().readShort();
				newBoss.yTo = msg.reader().readShort();
				newBoss.setFly();
			}
			if (actionBoss == 22)
			{
			}
			if (actionBoss == 23)
			{
				newBoss.setDie();
			}
		}
		catch (Exception)
		{
		}
	}
}
