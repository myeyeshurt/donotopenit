using System;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
namespace proj
{
    class Game
    {
		static LogWriter log = new LogWriter();
		static StatusWriter stat = new StatusWriter();
		
		private static Skill[,] PlayerSkill = 
		{
			{
				new Skill("Мощный удар", (State st) => st.CurEncounter.hp-=(int)(st.P0.dmg * 1.1), 3), 
				new Skill("Ярость", st => st.P0.dmg*=2, 15)
			},
			{
				new Skill("Магический Снаряд", st => st.CurEncounter.hp-=(int)(st.P0.mdmg*1.1), 1), 
				new Skill("Проклятие Смерти", 
					st => st.CurEncounter.hp -= (int)((0.9 + (st.CurEncounter.stats.Con*1.66 / st.CurEncounter.hp)) * st.P0.mdmg),
					2
				)
			},
			{
				new Skill("Удар в спину", st => st.CurEncounter.hp -= (int)(st.P0.dmg*(st.P0.critd/10.0)), 2), 
				new Skill("Невидимость", st => st.P0.eva*=5, 15)
			}
		};
		
		private static Skill[,] FoeS = 
		{
			{
				new Skill("Рывок", st => st.P0.hp-=(int)(st.CurEncounter.dmg*1.25), 4), 
				new Skill("Кража эссенции", 
					st => st.CurEncounter.hp+=(int)(st.P0.hp - (st.P0.hp-=(int)(st.CurEncounter.mdmg*1.1))), 5), 
				new Skill("Боевая стойка", st => st.CurEncounter.def=(int)(st.CurEncounter.def*1.66), 15)
			}
			,
			{
				new Skill ("Лечение", st => st.P0.hp+=5, 0),
				new Skill ("Проклятие", st => st.P0.hp-=5, 0),
				new Skill ("Благословение", st => st.P0.stats.Str += 2, 0)
			}
		};

		private static Skill Shop = new Skill("Магазин", 
		st =>
		{
			log.Write("Торговец хочет вам кое-что показать...\n", ConsoleColor.Blue);
			int [] IEmat = new int[3] {st.Dice.Next(0, 2), st.Dice.Next(0, 2), st.Dice.Next(0, 2)}, //я не знаю, почему тут 2/3 шанс на выдачу в
			Nummat = new int[3];																	//продажу шмотки. зачем для шансов нужен отдельный
			int i = 0;																				//массив тоже не ясно, но переделывать лучше не
			for (; i < 3; i++)																		//буду.
			{
				if (IEmat[i] == 0)
				{
					Nummat[i] = st.Dice.Next(0, Items.Length);
					log.Write(i + ") " + Items[Nummat[i]].name + " (10 з.)\n");
				}
				else
				{
					Nummat[i] = st.Dice.Next(0, ShopEquip.Length);
					log.Write(i + ") " + ShopEquip[Nummat[i]].name + " (100 з.)\n");
				}
			}
			log.Write("Сумма ваших сбережений: " + st.P0.gold + ".\n", ConsoleColor.Yellow);
			stat.Write("(Number) Space");
			var choise = 0;
			while((choise = Console.ReadKey().KeyChar) != ' ')
				if (choise < '3')
				{
					var intchoise = choise - '0';
					if (IEmat[intchoise] == 0)
					{
						if (st.P0.gold >= 10)
						{
							if (st.P0.sset[st.P0.sset.Length-1] != null)
								Array.Resize(ref st.P0.sset, st.P0.sset.Length+1);
							st.P0.sset[st.P0.sset.Length-1] = Items[Nummat[intchoise]];
							st.P0.gold -= 10;
							log.Write("Вы приобрели " + Items[Nummat[intchoise]].name + "!\n", ConsoleColor.Green);
						}
						else log.Write("У вас недостаточно золота.\n", ConsoleColor.Red);
					}
					else
					{
						if (st.P0.gold >= 100)
						{
							if (st.P0.equip[st.P0.equip.Length-1] != null)
								Array.Resize(ref st.P0.equip, st.P0.equip.Length+1);
							st.P0.equip[st.P0.equip.Length-1] = ShopEquip[Nummat[intchoise]];
							st.P0.gold -= 100;
							log.Write("Вы приобрели " + ShopEquip[Nummat[intchoise]].name + "!\n", ConsoleColor.Green);
						}
						else log.Write("У вас недостаточно золота.\n", ConsoleColor.Red);
					}
					break;
				}
		},
		0);


		
		
		private static Item [] Items = 
		{
			new Item ("Зелье", st => st.P0.hp+=10, 1, 1)
		};
		
		
		private static Equip
		WarWeap  = new Equip("Меч", (st, equiped) => st.P0.dmg+=(equiped?-5:5), false),
		WarArm   = new Equip("Латы", (st, equiped) => st.P0.def+=(equiped?-10:10), false),
		MageWeap = new Equip("Посох мага", (st, equiped) => st.P0.mdmg+=(equiped?-5:5), false),
		MageArm  = new Equip("Роба", (st, equiped) => st.P0.def+=(equiped?-2:2), false),
		RogueWeap= new Equip("Кинжал", (st, equiped) => {st.P0.dmg+=(equiped?-3:3); st.P0.critd+=(equiped?-5:5);}, false),
		RogueArm = new Equip("Кожанка", (st, equiped) => st.P0.def+=(equiped?-5:5), false);
		
		private static Equip[] ShopEquip = 
		{
			new Equip ("Длинный Меч Самурая", (st, equiped) => st.P0.dmg+=(equiped?-10:10), false),
			new Equip ("Кристальный кинжал", (st, equiped) => { st.P0.dmg+=(equiped?-7:7); st.P0.critd+=(equiped?-10:10); }, false),
			new Equip ("Демонический посох", (st, equiped) => st.P0.mdmg+=(equiped?-10:10), false),
			new Equip ("Полный латный доспех", (st, equiped) => st.P0.def+=(equiped?-25:25), false),
			new Equip ("Латный кожаный доспех", (st, equiped) => 
				{ st.P0.def+=(equiped?-10:10); st.P0.dmg+=(equiped?-4:4); st.P0.hp+=(equiped?2:-2);},
				false
			),
			new Equip ("Роба Кармиана", (st, equiped) => { st.P0.def+=(equiped?-6:6); st.P0.mdmg+=(equiped?-3:3); }, false)
		};

			
		
		private static string[,] objname = new string[,]
		{
			{
				"Летучая мышь",
				"Гоблин",
				"Скелет",
				"Монах",
				"Дух",
				"Змея"
			},
			{
				"Пирог",
				"Валун",
				"Сундук",
				"Фонтан",
				"Статуя",
				null
			}
		};
			
        static void Main(string[] args)
        {
			Player P;
			State GS;
			//Console.ReadKey();
			Console.Title = "";
			Console.OutputEncoding = new UnicodeEncoding(! BitConverter.IsLittleEndian, false);
			Console.Clear();
			Console.SetWindowSize(80, 43);
			Console.SetBufferSize(80, 43);
			
			string binpath = Environment.SpecialFolder.ApplicationData + "\\Jopasoft\\save";
			if (File.Exists(binpath))
			{
				IFormatter formatter = new BinaryFormatter();  
				Stream stream = new FileStream(binpath, FileMode.Open, FileAccess.Read, FileShare.Read);  
				SerializableState SS = (SerializableState) formatter.Deserialize(stream);  
				stream.Close();
				File.Delete(binpath);
				
				Skill[] sset = new Skill[SS.skills.Length+2], sset0;
				
				int j = 0;
				sset[j++] = PlayerSkill[SS.type,0];
				if (SS.lvl >= 3)
					sset[j++] = PlayerSkill[SS.type,1];
				for (int i = 0; i < SS.skills.Length; i++)
					if(SS.skills[i] >= 0)
						sset[j++] = Items[SS.skills[i]];
				sset0 = new Skill[j];
				Array.ConstrainedCopy(sset, 0, sset0, 0, j);
				j = 0;
				
				Equip[] eq = new Equip[SS.equip.Length+2], eq0;
				if(SS.equip.Length > 0)
				for (int i = 0; i < SS.equip.Length; i++)
					if(SS.equip[i] >= 0)
						eq[j++] = ShopEquip[SS.equip[i]];
				switch(SS.type)
				{
					case 0:
						eq[j++] = WarWeap;
						eq[j] = WarArm;
						break;
					case 1:
						eq[j++] = MageWeap;
						eq[j] = MageArm;
						break;
					case 2:
						eq[j++] = RogueWeap;
						eq[j] = RogueArm;
						break;
				}
				eq0 = new Equip[j+1];
				Array.ConstrainedCopy(eq, 0, eq0, 0, j+1);
				
				P = new Player(SS.name, SS.stats, (SS.type==2?15:13), sset0, eq0);
				P.type = SS.type;
				P.lvl = SS.lvl;
				P.exp = SS.exp;
				P.gold = SS.gold;
				P.dmg = SS.dmg;
				P.mdmg = SS.mdmg;
				P.hp = SS.hp;
				P.def = SS.def;
				P.init = SS.init;
				P.critc = SS.critc;
				P.eva = SS.eva;
				P.critd = SS.critd;
				GS = new State(P);
				GS.cooldowns = SS.cooldowns;
				GS.turn = SS.turn;
				GS.shopcore = SS.shopcore;
			}
			else
			{
			
				log.Write("Вы находите себя в просторном душном помещении. Протерев глаза, вы осознаёте, что лежите на полу подземелья. Вскакивая, полностью проснувшись, вы вспоминаете, что вы были... ");
				stat.Write("(M)age    (W)arrior    (R)ogue ");
				
				_firstchoice:
				switch(Console.ReadKey().KeyChar)
				{
					case 'M': case 'm':
						log.Write("магом.\n");
						P = new Player("Player", new Stats(12, 19, 29, 40), 13, new Skill[2]{PlayerSkill[1,0], null}, new Equip[2]{MageWeap, MageArm});
						P.type = 1;
						break;
					case 'W': case 'w':
						log.Write("воином.\n");
						P = new Player("Player", new Stats(32, 22, 38, 9), 13, new Skill[2]{PlayerSkill[0,0], null}, new Equip[2]{WarWeap, WarArm});
						P.type = 0;
						break;
					case 'R': case 'r':
						log.Write("плутом.\n");
						P = new Player("Player", new Stats(25, 33, 30, 12), 15, new Skill[2]{PlayerSkill[2,0], null}, new Equip[2]{RogueWeap, RogueArm
						});
						P.type = 2;
						break;
					default: 
						goto _firstchoice; 
				}
				GS = new State(P);
				
				GS.cooldowns = new int[2] {0,0};
				GS.P0.equip[0].wear(GS, GS.P0.equip[0].equiped);
				GS.P0.equip[0].equiped=true;
				GS.P0.equip[1].wear(GS, GS.P0.equip[1].equiped);
				GS.P0.equip[1].equiped=true;
			}

			while(GS.run)
			{
				Func<Creatura, Creatura> RecalcStats = (Creatura c) => {
					c.dmg = c.stats.Str;
					c.mdmg = c.stats.Int;
					c.hp = (int)(c.stats.Con * 2);
					c.def = c.stats.Con;
					c.init = (int)(c.stats.Dex/5);
					c.critc = (int)(c.stats.Dex * 1.33);
					c.eva = c.stats.Dex;
					return c;
				};
				Func<int, int> tento = (int n)  => (n>9?'a'+n-10:n+'0'); //если выборов будет внезапно больше 9, то дальше пойдёт латиница
				Func<int, int> toten = (int n)  => (n>'9'||n<'0'?n+10-'a':n-'0');
				
				if(GS.CurEncounter == null)
				{
					log.Write("Ваши действия?\n");
					stat.Write("(M)ove    (I)tem    (S)tatus    (Q)uit");
					_peaceloop:
					stat.SetAnswerCursor();
					switch(Console.ReadKey().KeyChar)
					{
						case 'M': case 'm':
							var enc = GS.Dice.Next(1, 21);
							var cell = -1;
							if (GS.shopcore > 0) GS.shopcore--;
							if (enc >= 6 && enc < 10)
								cell = 1;
							else if (enc >= 11 && enc < 17 + GS.shopcore)
								cell = 0;
							else if (enc >= 19 + GS.shopcore)
							{
								cell = 2;
								GS.shopcore = 5;
							}
							else log.Write("Вы проходите вперед.\n");
							if (cell > -1)
							{
								Foe Nobject = new Foe();
								Nobject.IsAggro = (cell==0);
								if (cell < 2)
									while(Nobject.name == null)
										Nobject.name = objname[cell, GS.Dice.Next(0, objname.GetLength(1))];
								else Nobject.name = "Торговца";
								Nobject.lvl = GS.Dice.Next(GS.P0.lvl-1, GS.P0.lvl+2) + (GS.Dice.Next(1, 21) < 5?1:0);
								int[] margin = new int[]{0,0};
								Nobject.stats = new Stats(0,0,0,0);
								margin[0] = Nobject.stats.Str = GS.Dice.Next(1, 51 + Nobject.lvl); //делим, чтоб сумма всех статов была 100+лвл*2
								Nobject.stats.Int = 100+Nobject.lvl*2 - (margin[1] = GS.Dice.Next(51 + Nobject.lvl, 101 + Nobject.lvl));
								Nobject.stats.Dex = GS.Dice.Next(margin[0], margin[1]) - Nobject.stats.Str;
								Nobject.stats.Con = margin[1] - Nobject.stats.Dex - margin[0];
								
								Nobject = RecalcStats(Nobject) as Foe;
								Nobject.critd = 13;
								int skcount = (cell == 0 ? GS.Dice.Next(0,FoeS.GetLength(1)+1) : 1);
								if (skcount > 0 && cell < 2) //случайные скиллы (кроме торговца)
								{
									int[] ss = new int[FoeS.GetLength(1)];
									for(int k = 0; k < ss.Length; k++)
										ss[k] = k;
									for (int k = 0; k < ss.Length; k++) //шаффл
									{
										int n = GS.Dice.Next(k+1);
										int s1 = ss[k];
										ss[k] = ss[n];
										ss[n] = s1;
									}
									Nobject.sset = new Skill[skcount];
									for (int k = 0; k < skcount; k++)
										Nobject.sset[k] = FoeS[cell,ss[k]];
								}
								else 
								{
									Nobject.sset = new Skill[1];
									if (cell == 2)
										Nobject.sset[0] = Shop;
								}
								GS.CurEncounter = Nobject;
							}
							break;
						case 'I': case 'i':
							log.Write("Вы заглядывете в инвентарь...\n", ConsoleColor.Green);
							int i = 0, j = 0, count = 0;
							for (; i < GS.P0.equip.Length; i++)
								if (GS.P0.equip[i] != null)
								{
									log.Write ((char)tento(i) + ") " + GS.P0.equip[i].name + (GS.P0.equip[i].equiped?"(надет)":"") + "\n");
									count++;
								}
							for (; j < GS.P0.sset.Length; j++)
								if (GS.P0.sset[j] != null && GS.P0.sset[j] is Item && ((Item)GS.P0.sset[j]).usecount > 0) //расходники в скиллах, но 
								{																						//игроку всё-таки надо вывести
									log.Write((char)tento(i++) + ") " + GS.P0.sset[j].name + "\n");
									count++;
								}
								else i++;
							log.Write("Использовать?\n");
							stat.Write("(Item)  Space");
							int usechoise;
							_useloop:
							stat.SetAnswerCursor();
							if((usechoise = Console.ReadKey().KeyChar) != ' ')
							{
								if ((usechoise = toten(usechoise)) < count && usechoise >= 0)
								{
									if (usechoise < (i-j))
									{
										GS.P0.equip[usechoise].wear(GS, GS.P0.equip[usechoise].equiped);
										GS.P0.equip[usechoise].equiped=!GS.P0.equip[usechoise].equiped;
										log.Write("Вы использовали " + GS.P0.equip[usechoise].name + "\n", ConsoleColor.Cyan);
									}
									else
									{
										GS.P0.sset[usechoise-(i-j)].act(GS);
										if (--((Item)GS.P0.sset[usechoise-(i-j)]).usecount == 0)
											GS.P0.sset[usechoise - (i-j)] = null;
										log.Write("Вы использовали " + GS.P0.sset[usechoise-(i-j)].name + "\n", ConsoleColor.Cyan);
									}
								}
								else goto _useloop;
							}
							else GS.turn--;
							break;
						case 'S': case 's':
							log.Write("Вы осматриваете себя...\n", ConsoleColor.Green);
							log.Write("HP: " + GS.P0.hp + "\nУрон: " + GS.P0.dmg + "\nМаг. урон: " + GS.P0.mdmg);
							log.Write("\nЗащита: " + GS.P0.def + "\nУклонение: " + GS.P0.eva + "\nШанс крита: " + GS.P0.critc);
							log.Write("\nКритический урон: " + GS.P0.critd + "\n");
							log.Write("Готовые умения: ");
							foreach(Skill sk in GS.P0.sset)
								if (sk != null && !(sk is Item /*&& ((Item)sk).usecount > -1*/))
									log.Write(sk.name + " ");
							log.Write(".\n");
							log.Write("Ходов сделано: " + GS.turn + "\n");
							GS.turn--;
							break;
						case 'Q': case 'q':
							log.Write("Хотите сохранить прогресс?\n");
							stat.Write("(Y)es    (N)o");
							switch(Console.ReadKey().KeyChar)
							{
								case 'Y': case 'y':
									SerializableState SS = new SerializableState(GS, ShopEquip, Items);
									IFormatter formatter = new BinaryFormatter();  
									Directory.CreateDirectory(Path.GetDirectoryName(binpath));
									Stream stream = new FileStream(binpath, FileMode.Create, FileAccess.Write, FileShare.None);  
									formatter.Serialize(stream, SS); 
									stream.Close(); 
									break;
							}
							GS.run = false;
							break;
						default: goto _peaceloop;
					}
				}
				else
				{
					bool DidCrit;
					int buffer;
					
					log.Write("На своём пути вы встретили " + GS.CurEncounter.name + 
					"(" /*+ GS.CurEncounter.stats.Str + " " +  GS.CurEncounter.stats.Dex + " " + GS.CurEncounter.stats.Con + " " +
					GS.CurEncounter.stats.Int + " " */+ GS.CurEncounter.lvl + " уровня)!\n", ConsoleColor.Red);
					/*log.Write("Скиллы: ");
					foreach(Skill s in GS.CurEncounter.sset)
					if (s != null)
						log.Write(s.name + ", ");
					log.Write("\n");*/
					//GS.CurEncounter = null;
					stat.Write("(A)ttack    (R)etreat    (U)se");
					_encounterloop:
					stat.SetAnswerCursor();
					switch(Console.ReadKey().KeyChar)
					{
						case 'R': case 'r':
							if(!GS.CurEncounter.IsAggro)
							{
								log.Write("Вы проходите мимо.\n");
								GS.CurEncounter = null;
								break;
							}
							else
							{
								if (GS.Dice.Next(1, GS.P0.init+1) + GS.Dice.Next(3) > GS.CurEncounter.init)
								{
									log.Write("Вы успешно сбежали.\n");
									GS.CurEncounter = null;
									break;
								}
								else 
								{
									log.Write("Вы не смогли сбежать.\n", ConsoleColor.Red);
									GS.CurEncounter.init = 20;
									goto case 'a';
								}
							}
						case 'U': case 'u':
							if(GS.CurEncounter.IsAggro)
							{
								log.Write("Нельзя это использовать!\n", ConsoleColor.Red);
								goto case 'a';
							}
							else
							{
								log.Write("Вы дотрагиваетесь до " + GS.CurEncounter.name + "... ");
								Skill ts;
								if((ts = GS.CurEncounter.sset[0]) != null)
								{
									log.Write("происходит " + ts.name + "!\n", ConsoleColor.Green);
									ts.act(GS);
								}
								else log.Write("ничего не произошло.\n");
								GS.CurEncounter = null;
								break;
							}
						case 'A': case 'a':
							if(GS.CurEncounter.IsAggro)
							{
								if(GS.CurEncounter.init > GS.P0.init)
								{
									if ((buffer = GS.CurEncounter.dmg - GS.Dice.Next(1, GS.P0.def+1)) < 0) //без крита и уклонения, если не прошло
										buffer = 0;														// проверку на инициативу
									GS.P0.hp -= buffer;
									log.Write(GS.CurEncounter.name + " наносит " + buffer + " урона (" + GS.P0.hp + ")\n");
								}
									
								stat.Write("(A)ttack    (D)efend    (S)kill");
								while(GS.P0.hp > 0 && GS.CurEncounter.hp > 0) //бой с аггром
								{
									bool skillUsed = false;
									stat.SetAnswerCursor();
									switch(Console.ReadKey().KeyChar)
									{
										case 'A': case 'a':
											if (!skillUsed)
											if(GS.CurEncounter.eva/2.0 < GS.Dice.Next(1, 101))
											{		
												if((buffer = 
													((int)(GS.P0.dmg * 
														((DidCrit = GS.Dice.Next(1, 101) < GS.P0.critc)?
														GS.P0.critd/10.0 : 1.0)) -
													GS.Dice.Next(1, GS.CurEncounter.def+1))) < 0 		//урон = дмг * крит - (1--деф), шанс крита
												)														//в процентах
													buffer = 0;
												GS.CurEncounter.hp -= buffer;
												log.Write((DidCrit?"Критический удар! ":"") + "Вы нанесли " 
													+ GS.CurEncounter.name + " " + buffer + " урона. (" + GS.CurEncounter.hp +")\n");
											}
											else
												log.Write(GS.CurEncounter.name + " уклоняется.\n");
											if (GS.CurEncounter.hp < 1)
												break;
											if (GS.Dice.Next(0, 10) == 9) //10 процентов, что моб вместо атаки юзает скилл
											{
												int n = GS.Dice.Next(0, GS.CurEncounter.sset.Length);
												if (GS.CurEncounter.sset[n] != null && GS.CurEncounter.sset[n].cooldown > 0)
												{
													GS.CurEncounter.sset[n].act(GS);
													GS.CurEncounter.sset[n].cooldown = 0;
													log.Write(GS.CurEncounter.name + " применяет " + "\"" + GS.CurEncounter.sset[n].name +"\".\n");
												}
											}
											else 
											{
												if(GS.P0.eva/2.0 < GS.Dice.Next(1, 101))
												{		
													if ((buffer = 
														((int)(GS.CurEncounter.dmg * 
															((DidCrit = GS.Dice.Next(1, 101) < GS.CurEncounter.critc)?
															GS.CurEncounter.critd/10.0 : 1.0)) -
														GS.Dice.Next(1, GS.P0.def+1))) < 0
													)
														buffer = 0;
													GS.P0.hp -= buffer;
													log.Write((DidCrit?"Критический удар! ":"") + GS.CurEncounter.name +" наносит " 
													+ buffer + " урона. (" + GS.P0.hp + ")\n");
												}
												else
													log.Write("Вы ушли от атаки!\n");
											}
										break;
										case 'D': case 'd':
											if(GS.P0.eva/1.75 < GS.Dice.Next(1, 101))
											{		
												if ((buffer = 
													((int)(GS.CurEncounter.dmg * 
														((DidCrit = GS.Dice.Next(1, 101) < GS.CurEncounter.critc)?
														GS.CurEncounter.critd/10.0 : 1.0)) -
													GS.P0.def)) < 0
												)
													buffer = 0;
												GS.P0.hp -= buffer;
												log.Write((DidCrit?"Критический удар! ":"") + GS.CurEncounter.name +" наносит " 
												+ buffer + " урона (" + GS.P0.hp + ")\n");
											}
											else
												log.Write("Вы ушли от атаки!\n");
										break;
										case 'S': case 's':
										{
											for (int i = 0; i < GS.P0.sset.Length; i++)
												if (GS.P0.sset[i] != null)
													log.Write(((char)tento(i)) + ") " + GS.P0.sset[i].name + "\n");
											stat.Write("(Skill)    Space");
											int usechoise;
											while((usechoise = Console.ReadKey().KeyChar) != ' ')
											{
												if ((usechoise = toten(usechoise)) < GS.P0.sset.Length 
												&& usechoise >= 0 && GS.P0.sset[usechoise] != null )
												{
													//log.Write(usechoise.ToString() + "\n");
													if (GS.P0.sset[usechoise] is Item || GS.cooldowns[usechoise] == 0)
													{
														GS.P0.sset[usechoise].act(GS);

														log.Write("Вы применяете " + GS.P0.sset[usechoise].name + ".\n", ConsoleColor.Cyan);
														if(GS.P0.sset[usechoise] is Item)
														{
															if (--((Item)GS.P0.sset[usechoise]).usecount == 0)
																GS.P0.sset[usechoise] = null;
														}
														else
															GS.cooldowns[usechoise] = GS.P0.sset[usechoise].cooldown;
														skillUsed = true; //после скилла даём мобу себя ударить
														break;
													}
													else 
													{
														log.Write("Умение откатывается.\n");
														GS.turn--;
														break;
													}
												}

											}
											stat.Write("(A)ttack    (D)efend    (S)kill");
											goto case 'a';
											//break;
										}
									}
									GS.turn++;
									for (int i = 0; i < GS.cooldowns.Length; i++)
									if (GS.cooldowns[i] > 0)
										GS.cooldowns[i]--;
								}
								if (GS.P0.hp < 1)
									GS.run = false;
								else 
								{
									int varholder;
									log.Write("Вы убиваете " + GS.CurEncounter.name + "!\n", ConsoleColor.Yellow);
									GS.P0.exp += ( varholder = (GS.Dice.Next(((GS.CurEncounter.lvl * 10) + 2)/ 2, (GS.CurEncounter.lvl * 10) + 1)) );
									//даёт (((лвл*10)+2)/2)~((лвл*10)+1) опыта (хотя бы одно очко даст)
									log.Write("Вы получаете " + varholder + " ОО", ConsoleColor.Yellow);
									GS.P0.gold += ( varholder = (GS.Dice.Next(20) > 4 ? 10 + GS.CurEncounter.lvl*7 + GS.Dice.Next(5) : 0) );
									//с шансом 4/5 даёт 10~15 + лвл*7 голды
									if (varholder > 0)
										log.Write(" и " + varholder + " золота", ConsoleColor.Yellow);
									log.Write(".\n");
									GS.CurEncounter = null;
								}
							}
							else
							{
								int varholder;
								GS.turn += (varholder  = ((GS.P0.dmg/GS.CurEncounter.def) - 1)); //мотаем ходы, пока бьём неаггра
								GS.P0.hp += (int)Math.Truncate(varholder/2.0);
								//deathrattle
								log.Write("Вы уничтожаете " + GS.CurEncounter.name + ".\n");
								GS.CurEncounter = null;
							}
						break;
						default: goto _encounterloop;
					}
				}
				GS.turn++;
				if (GS.turn%2 == 0)
					GS.P0.hp ++;
				bool entered = false;
				while (GS.P0.exp >= Math.Pow(GS.P0.lvl, 4))
				{
					entered = true;
					GS.P0.lvl++;
					for (int i = 0; i < 2; i++)
						switch(GS.Dice.Next(3))
						{
							case 0: GS.P0.stats.Str++;
							break;
							case 1: GS.P0.stats.Dex++;
							break;
							case 2: GS.P0.stats.Con++;
							break;
							case 3: GS.P0.stats.Int++;
							break;
						}
					GS.P0 = RecalcStats(GS.P0) as Player;
					foreach(Equip e in GS.P0.equip)
						if (e.equiped)
							e.wear(GS, false);
					if (GS.P0.lvl == 3)
					{
						if (GS.P0.sset[GS.P0.sset.Length-1] != null)
						{
							Array.Resize(ref GS.P0.sset, GS.P0.sset.Length+1);
							Array.Resize(ref GS.cooldowns, GS.cooldowns.Length+1);
						}
						GS.P0.sset[GS.P0.sset.Length-1] = PlayerSkill[GS.P0.type,1];
					}
					if (GS.P0.lvl == 5)
						GS.run = false;
					log.Write("Уровень персонажа повысился\n", ConsoleColor.Yellow);
				}
				if(!entered) //надо пересчитать статы, если не было лвлапа (для селфбаффов)
				{				
					int temp = GS.P0.hp;
					GS.P0 = RecalcStats(GS.P0) as Player;
					if (temp < GS.P0.hp)
						GS.P0.hp = temp;
				}
				
				for (int i = 0; i < GS.cooldowns.Length; i++)
					if (GS.cooldowns[i] > 0)
						GS.cooldowns[i]--;
			}
			if (GS.P0.lvl == 5)
				log.Write("\nСтены подземелья начинают страшно трястись. Вы поворачиваетесь... и видите знакомый интерьер своей комнаты. Под подушкой вибрирует мобила, вы выключаете будильник. \"Вау, надо меньше играть в DCSS... приснится же, какой отвратительный дизайн и никакого баланса -- это действительно было похоже на кошмар!\" -- думаете вы и встаёте с кровати: через полчаса нужно снова быть на парах...\n");
			else if (GS.P0.hp <= 0)
				log.Write("\nВы умерли в подземелье. Ваши подвиги никто не запомнит; может, какой-нибудь такой же искатель приключений, найдя ваши кости, помолится мимоходом, но не более. Постарайтесь лучше в следующей жизни.\n");
        }
    }
	
	class State
	{
		public bool run;
		public int turn;
		
		
		public Random Dice;
		
		public Player P0;
		public int[] cooldowns;
		
		public Foe CurEncounter;
		
		public int shopcore; //чтоб магаз не попадался слишком часто
		
		public State(Player player)
		{
			run = true;
			turn = 0;
			Dice = new Random();
			P0 = player;
			CurEncounter = null;
		}
	}
	[Serializable()]
	class Stats
	{
		public int Str; //dmg
		public int Dex; //init, crit.c
		public int Con; //hp
		public int Int; //mdmg
		//public int Wit; //
		public Stats (int S, int D, int C, int I/*, int W*/) { Str=S; Dex=D; Con=C; Int=I; /*Wit=W;*/ }
	}
	
	class Skill
	{
		public string name;
		public delegate void Act (State game);
		public Act act;
		public int cooldown;
		public Skill(string n, Act a, int cooldown)
		{
			name = n; 
			act = a;
			this.cooldown = cooldown;
		}
	}
	
	class Item : Skill
	{
		public int usecount;
		public Item (string n, Act a, int cooldown, int usecount) :
			base(n, a, cooldown)
		{ this.usecount = usecount; }
	}
	
	class Equip
	{
		public string name;
		public delegate void Wear (State game, bool equiped);
		public Wear wear;
		public bool equiped;
		public Equip (string name, Wear wear, bool equiped)
		{
			this.name = name;
			this.wear = wear;
			this.equiped = equiped;
		}
	}
	
	abstract class Creatura
	{
		public string name;
		public int lvl;
		public Stats stats;
		public int dmg, mdmg, hp, def, init, critc, eva, critd; //calculate once|current
		//[NonSerialized()]
		public Skill[] sset;
	}
	
	class Player : Creatura
	{
		public int exp;
		public int gold;
		public int type;
		//[NonSerialized()]
		public Equip[] equip;
		public Player (string name, Stats Sts, int critd, Skill[] Sks, Equip[] eq)
		{
			this.name = name;
			lvl = 1;
			stats = Sts;
			sset = Sks;
			equip = eq;
			dmg = Sts.Str;
			mdmg = Sts.Int;
			hp = (int)(Sts.Con * 2);
			def = Sts.Con;
			init = (int)(Sts.Dex/5);
			critc = (int)(Sts.Dex * 1.33);
			eva = Sts.Dex;
			this.critd = critd;
			
			exp = 0;
			gold = 0;
		}
	}
	
	class Foe : Creatura
	{
		public bool IsAggro;
	}
	
	
	interface FancyConsole
	{
		public void ClearLine(int n);
		public void Write(string message);
	}
	
	class LogWriter : FancyConsole
	{
		public int posx, posy;
		public void Write(string message, ConsoleColor col)
		{
			Console.SetCursorPosition(posx, posy);
			int PostPosY = posy;
			int MesLen = 0;
			int i0 = 0, i1 = 0;
			int diff;
			
			while((i0 = message.IndexOf('\n', i1)) >= 0) //я уже забыл, что тут происходит... какие-то расчёты для удаления верхних строк
			{											//(нет скроллбека)
				if((i1==0?posx:0) + (i0 - i1) < 80)
					MesLen += 80;
				else MesLen += 160;
				i1 = i0+1;
			}
			MesLen += message.Length-i1;
			PostPosY += (diff = (int)(Math.Truncate(MesLen/80.0)));
			
			if(PostPosY > 41)
			{
				Console.MoveBufferArea(0, diff, 80, posy - diff + 1, 0, 0);
				Console.SetCursorPosition(posx, posy - diff);
			}
			Console.ForegroundColor = col;
			Console.Write(message);
			posx = Console.CursorLeft;
			posy = Console.CursorTop;
		}
		public void Write(string message)
		{
			Write(message, ConsoleColor.White);
		}
		public void ClearLine(int n)		
		{
				Console.SetCursorPosition(0, n); 
				Console.Write(new string(' ', Console.WindowWidth)); 
				Console.SetCursorPosition(0, n);
		}
		public LogWriter() { posx = 0; posy = 0; }
	}
	
	class StatusWriter : FancyConsole
	{
		public int posx;
		public void ClearLine(int n) 
		{
			Console.SetCursorPosition(0, n); 
			Console.Write(new string(' ', Console.WindowWidth-1)); 
			Console.SetCursorPosition(0, n);
		}
		public void Write(string message)
		{
			ClearLine(42);
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.Write(message);
			//Console.ForegroundColor = ConsoleColor.White;
			posx = Console.CursorLeft;
		}
		public void SetAnswerCursor()
		{
			Console.SetCursorPosition(posx + 1, 42);
		}
		public StatusWriter () { posx = 0; }
	}
	
	[Serializable()]
	class SerializableState //неткор не может в сериализацию делегатов, а самому делать нетривиально, поэтому так
	{
		public int turn;
		
		public int[] cooldowns;
		
		public int shopcore;
		
		public int exp;
		public int gold;
		public int type;
		
		public int[] equip;
		public int[] skills;
		
		public string name;
		public int lvl;
		public Stats stats;
		public int dmg, mdmg, hp, def, init, critc, eva, critd;
		public SerializableState(State GS, Equip[] ShopEquip, Item[] Items)
		{
			turn = GS.turn;
			shopcore = GS.shopcore;
			cooldowns = GS.cooldowns;
			exp = GS.P0.exp;
			gold = GS.P0.gold;
			type = GS.P0.type;
			equip = new int[GS.P0.equip.Length];
			skills = new int[GS.P0.sset.Length];
			for (int i = 0; i < GS.P0.equip.Length; i++)
				if (GS.P0.equip[i] != null)
					equip[i] = Array.FindIndex(ShopEquip, x => x.name == GS.P0.equip[i].name);

			for (int i = 0; i < GS.P0.sset.Length; i++)
				if (GS.P0.sset[i] != null)
					skills[i] = Array.FindIndex(Items, x => x.name == GS.P0.sset[i].name);
			name = GS.P0.name;
			lvl = GS.P0.lvl;
			stats = GS.P0.stats;
			dmg = GS.P0.dmg;
			mdmg = GS.P0.mdmg;
			hp = GS.P0.hp;
			def = GS.P0.def;
			init = GS.P0.init;
			critc = GS.P0.critc;
			eva = GS.P0.eva;
			critd = GS.P0.critd;
		}
	}
}
