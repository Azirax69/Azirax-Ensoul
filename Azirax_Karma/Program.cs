namespace Azirax_Karma
{
    using System;
    using System.Linq;
    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.MenuUI;
    using EnsoulSharp.SDK.MenuUI.Values;
    using EnsoulSharp.SDK.Prediction;
    using EnsoulSharp.SDK.Utility;
    using Color = System.Drawing.Color;

    public class Program
    {
        private static Menu MainMenu;

        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell R;

        private static void Main(string[] args)
        {

            GameEvent.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad()
        {

            if (ObjectManager.Player.CharacterName != "Karma")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 950);
            W = new Spell(SpellSlot.W, 675);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.E, 1100);

            Q.SetSkillshot(250f, 100f, 1500f, true, SkillshotType.Line);
            W.SetTargetted(0.25f, float.MaxValue);
            E.SetTargetted(0.25f, float.MaxValue);





            MainMenu = new Menu("aziraxkarma", "Azirax Karma", true);


            var comboMenu = new Menu("Combo", "Combo");
            comboMenu.Add(new MenuBool("comboQ", "Use Q", true));
            comboMenu.Add(new MenuBool("comboW", "Use W", true));
            comboMenu.Add(new MenuBool("comboR", "Use R", true));
            MainMenu.Add(comboMenu);

            var harassMenu = new Menu("Harass", "Harass");
            harassMenu.Add(new MenuBool("harassQ", "Use Q", true));
            harassMenu.Add(new MenuBool("harassR", "Use R", true));
            harassMenu.Add(new MenuSlider("ManaHarass", "Harass ManaPercent", 60, 0, 100));
            MainMenu.Add(harassMenu);

            // vi essa aba misc no sharp e decidi colocar aqui também, o skin changer ainda não funciona, mas vou arrumar isso.
            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.Add(new MenuBool("Eshield", "Make E Shield"));
            miscMenu.Add(new MenuBool("egapclose", "Use E on Gapclosers"));
            miscMenu.Add(new MenuBool("qgapclose", "Use Q on Gapclosers"));
            miscMenu.Add(new MenuBool("skinHack", "Skin Change"));
            miscMenu.Add(new MenuSlider("SkinID", "Skin", 0, 0, 8));
            MainMenu.Add(miscMenu);

            var drawMenu = new Menu("Draw", "Draw");
            drawMenu.Add(new MenuBool("qRange", "Q range", false));
            drawMenu.Add(new MenuBool("wRange", "W range", false));
            drawMenu.Add(new MenuBool("eRange", "E range", false));
            drawMenu.Add(new MenuBool("skillReady", "Draw when skill ready", true));


            MainMenu.Attach();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Gapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Chat.Print("Azirax Karma, Any problem, report in my discord ;-;");

        }

        // Combos sendo corrigidos aos poucos. Azirax#5495
        private static void Combo()
        {
            var qTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(W.Range, DamageType.Magical);

            if (MainMenu["Combo"]["comboW"].GetValue<MenuBool>().Enabled && W.IsReady())
            {
                if ((ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) /
                    (qTarget.Health / qTarget.MaxHealth) < 1)
                {
                    if (MainMenu["Combo"]["comboR"].GetValue<MenuBool>().Enabled && R.IsReady())
                    {
                        R.Cast();
                    }

                    if (MainMenu["Combo"]["comboR"].GetValue<MenuBool>().Enabled && R.IsReady())
                    {
                        W.Cast(wTarget);
                    }
                }
            }

            if (MainMenu["Combo"]["comboQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
            {
                if (MainMenu["Combo"]["comboR"].GetValue<MenuBool>().Enabled && R.IsReady())
                {
                    R.Cast();
                }

                if (MainMenu["Combo"]["comboR"].GetValue<MenuBool>().Enabled && R.IsReady())
                {
                    var qPrediction = Q.GetPrediction(qTarget);
                    if (qPrediction.Hitchance >= HitChance.High)
                    {
                        Q.Cast(qPrediction.CastPosition);
                    }
                    else if (qPrediction.Hitchance == HitChance.Collision)
                    {
                        var minionsHit = qPrediction.CollisionObjects;
                        var closest =
                            minionsHit.Where(m => m.NetworkId != ObjectManager.Player.NetworkId)
                                .OrderBy(m => m.Distance(ObjectManager.Player))
                                .FirstOrDefault();

                        if (closest != null && closest.Distance(qPrediction.UnitPosition) < 200)
                        {
                            Q.Cast(qPrediction.CastPosition);
                        }
                    }
                }
            }

            if (MainMenu["Combo"]["comboW"].GetValue<MenuBool>().Enabled && wTarget != null)
            {
                W.Cast(wTarget);
            }

        }

        private static void Harass()
        {
            var qTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (MainMenu["Harass"]["harassQ"].GetValue<MenuBool>().Enabled && qTarget != null && Player.Instance.ManaPercent < MainMenu["Harass"].GetValue<MenuSlider>("ManaHarass").Value)
            {

                if (MainMenu["Harass"]["harassR"].GetValue<MenuBool>().Enabled && R.IsReady())
                {
                    R.Cast();
                }

                if (MainMenu["Harass"]["harassR"].GetValue<MenuBool>().Enabled && R.IsReady())
                {
                    var qPrediction = Q.GetPrediction(qTarget);
                    if (qPrediction.Hitchance >= HitChance.High)
                    {
                        Q.Cast(qPrediction.CastPosition);
                    }

                    else if (qPrediction.Hitchance == HitChance.Collision)
                    {
                        var minionsHit = qPrediction.CollisionObjects;
                        var closest =
                            minionsHit.Where(m => m.NetworkId != ObjectManager.Player.NetworkId)
                                .OrderBy(m => m.Distance(ObjectManager.Player))
                                .FirstOrDefault();

                        if (closest != null && closest.Distance(qPrediction.UnitPosition) < 200)
                        {
                            Q.Cast(qPrediction.CastPosition);
                        }


                    }

                }
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, Gapcloser.GapcloserArgs args)

        {
            if (!MainMenu["Misc"].GetValue<MenuBool>("egapclose"))
                return;
            var attacker = sender;
            if (attacker.IsValidTarget(300f))
            {
                E.Cast(ObjectManager.Player);

            }

            if (!MainMenu["Misc"].GetValue<MenuBool>("qgapclose"))
                return;
            var attacker1 = sender;
            if (attacker.IsValidTarget(300f))
            {
                Q.Cast(ObjectManager.Player);

            }
        }

        public static void ExecuteAdditionals()
        {
            if (MainMenu["Misc"]["Eshield"].GetValue<MenuBool>().Enabled && E.IsReady())
            {
                foreach (var hero in
                    ObjectManager.Get<AIHeroClient>()
                        .Where(
                            hero =>
                                hero.IsValidTarget(E.Range, false) && hero.IsAlly &&
                                ObjectManager.Get<AIHeroClient>().Count(h => h.IsValidTarget() && h.Distance(hero) < 400) >
                                1))
                {
                    E.Cast(hero);
                }
            }
        }



        private static void OnUpdate(EventArgs args)
        {
            ExecuteAdditionals();
            switch (Orbwalker.ActiveMode)
            {    
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
            }
        }

        private static void OnDraw(EventArgs args)
        {

            if (MainMenu["Draw"]["qRange"].GetValue<MenuBool>().Enabled)
            {
                if (MainMenu["skillReady"].GetValue<MenuBool>().Enabled)
                {
                    if (Q.IsReady())
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.Cyan);
                }

                else
                    Render.Circle.DrawCircle(Player.Instance.Position, Q.Range, Color.Cyan);

            }

            if (MainMenu["Draw"]["wRange"].GetValue<MenuBool>().Enabled)
            {
                if (MainMenu["skillReady"].GetValue<MenuBool>().Enabled)
                {
                    if (W.IsReady())
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, Color.Orange);
                }

                else
                    Render.Circle.DrawCircle(Player.Instance.Position, Q.Range, Color.Orange);

            }


            if (MainMenu["Draw"]["eRange"].GetValue<MenuBool>().Enabled)
            {
                if (MainMenu["skillReady"].GetValue<MenuBool>().Enabled)
                {
                    if (E.IsReady())
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.White);
                }
                else
                    Render.Circle.DrawCircle(Player.Instance.Position, E.Range, Color.White);
            }
        }
    }
}

