using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using NicoMessy.Miscellany.Items.Materials;
using NicoMessy.Miscellany.Systems;

namespace NicoMessy.Miscellany.Items.Weapons
{
	public class Microphone : ModItem
	{
        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true; // This makes the useStyle animate as a staff instead of as a gun.
        }
        public override void SetDefaults()
		{
            // Damage
            Item.damage = 41;
            Item.mana = 27;
			Item.DamageType = DamageClass.Magic;
            Item.useTime = 12;
            Item.reuseDelay = 9;
            Item.useAnimation = 45;
            Item.knockBack = 1.1f;
            Item.crit = 16;

			// Projectile Call
			Item.shoot = ProjectileID.PurificationPowder; // For some reason, all the guns in the vanilla source have this.
            Item.shootSpeed = 11.7f;

            // Sprite Stuff
            Item.width = 32;
			Item.height = 32;
            Item.scale = 1.1f;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.holdStyle = ItemHoldStyleID.None;
            Item.rare = ItemRarityID.LightRed;
			
			// Others
			Item.value = Item.sellPrice(0,12,23,21);
			Item.UseSound = SoundID.Item26;
			Item.autoReuse = true;
		}
        //This actually allows the thing to shoot something that isn't Purification Powder
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = Main.rand.Next(new int[] { ModContent.ProjectileType<Arrowleft>(), ModContent.ProjectileType<Arrowdown>(), ModContent.ProjectileType<Arrowup>(), ModContent.ProjectileType<Arrowright>() });
        }

        public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddRecipeGroup("NicoMessy:GoldBars", 25);
            recipe.AddRecipeGroup("IronBar", 30);
            recipe.AddIngredient(ItemID.ManaCrystal, 3);
            recipe.AddIngredient<MessyEssence>(4);
            recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
    }

    public abstract class ArrowBase : ModProjectile
    {
        public override string Texture => "NicoMessy/Miscellany/Projectiles/Arrows/";
        public override void SetDefaults()
        {

            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.scale = 1.05f;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 960;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 0;

        }

        private const int BufferSize = 10; // This is the amount of positions we'll store. 10 is one sixth of a second's worth, if you want more than that, just increase this value.
        private Vector2[] positionBuffer = new Vector2[BufferSize]; // Initialises a Vector2 array of size BufferSize (so 10).
        private int bufferTail = 0; // This keeps track of the 'tail' of the buffer. Don't worry, I'll explain below.
        private bool bufferFull; // This keeps track of whether or not the buffer has been filled yet.
        public abstract int ArrowDust { get; }

        public override void AI()
        {
            // Dust Effect AI
            positionBuffer[bufferTail] = Projectile.position;
            bufferTail++;
            if (bufferTail >= BufferSize)
            {
                bufferFull = true;
                bufferTail = 0;
            }

            int dustAmount = bufferFull ? BufferSize : bufferTail;

            for (int i = 0; i < dustAmount; i++)
            {
                int dustId = Dust.NewDust(positionBuffer[i], 1, 1, ArrowDust, 0f, 0f, 0, default(Color), 0.55f);
                Main.dust[dustId].alpha = Projectile.alpha;
                Main.dust[dustId].velocity *= 0f;
                Main.dust[dustId].noGravity = true;
                Main.dust[dustId].fadeIn *= 1.8f;
                Main.dust[dustId].scale = 1f;
            }

            // Homing AI
            float num132 = (float)Math.Sqrt((double)(Projectile.velocity.X * Projectile.velocity.X + Projectile.velocity.Y * Projectile.velocity.Y));
            float num133 = Projectile.localAI[0];
            if (num133 == 0f)
            {
                Projectile.localAI[0] = num132;
                num133 = num132;
            }
            float num134 = Projectile.position.X;
            float num135 = Projectile.position.Y;
            float num136 = 300f;
            bool flag3 = false;
            int num137 = 0;
            if (Projectile.ai[1] == 0f)
            {
                for (int num138 = 0; num138 < 200; num138++)
                {
                    if (Main.npc[num138].CanBeChasedBy(this, false) && (Projectile.ai[1] == 0f || Projectile.ai[1] == (float)(num138 + 1)))
                    {
                        float num139 = Main.npc[num138].position.X + (float)(Main.npc[num138].width / 2);
                        float num140 = Main.npc[num138].position.Y + (float)(Main.npc[num138].height / 2);
                        float num141 = Math.Abs(Projectile.position.X + (float)(Projectile.width / 2) - num139) + Math.Abs(Projectile.position.Y + (float)(Projectile.height / 2) - num140);
                        if (num141 < num136 && Collision.CanHit(new Vector2(Projectile.position.X + (float)(Projectile.width / 2), Projectile.position.Y + (float)(Projectile.height / 2)), 1, 1, Main.npc[num138].position, Main.npc[num138].width, Main.npc[num138].height))
                        {
                            num136 = num141;
                            num134 = num139;
                            num135 = num140;
                            flag3 = true;
                            num137 = num138;
                        }
                    }
                }
                if (flag3)
                {
                    Projectile.ai[1] = (float)(num137 + 1);
                }
                flag3 = false;
            }
            if (Projectile.ai[1] > 0f)
            {
                int num142 = (int)(Projectile.ai[1] - 1f);
                if (Main.npc[num142].active && Main.npc[num142].CanBeChasedBy(this, true) && !Main.npc[num142].dontTakeDamage)
                {
                    float num143 = Main.npc[num142].position.X + (float)(Main.npc[num142].width / 2);
                    float num144 = Main.npc[num142].position.Y + (float)(Main.npc[num142].height / 2);
                    if (Math.Abs(Projectile.position.X + (float)(Projectile.width / 2) - num143) + Math.Abs(Projectile.position.Y + (float)(Projectile.height / 2) - num144) < 1000f)
                    {
                        flag3 = true;
                        num134 = Main.npc[num142].position.X + (float)(Main.npc[num142].width / 2);
                        num135 = Main.npc[num142].position.Y + (float)(Main.npc[num142].height / 2);
                    }
                }
                else
                {
                    Projectile.ai[1] = 0f;
                }
            }
            if (!Projectile.friendly)
            {
                flag3 = false;
            }
            if (flag3)
            {
                float num145 = num133;
                Vector2 vector10 = new Vector2(Projectile.position.X + (float)Projectile.width * 0.5f, Projectile.position.Y + (float)Projectile.height * 0.5f);
                float num146 = num134 - vector10.X;
                float num147 = num135 - vector10.Y;
                float num148 = (float)Math.Sqrt((double)(num146 * num146 + num147 * num147));
                num148 = num145 / num148;
                num146 *= num148;
                num147 *= num148;
                int num149 = 8;
                Projectile.velocity.X = (Projectile.velocity.X * (float)(num149 - 1) + num146) / (float)num149;
                Projectile.velocity.Y = (Projectile.velocity.Y * (float)(num149 - 1) + num147) / (float)num149;
            }
        }

    }
    public class Arrowleft : ArrowBase
    {
        public override string Texture => base.Texture + "arrowleft";
        public override int ArrowDust => DustID.HallowedTorch;
    }
    public class Arrowdown : ArrowBase
    {
        public override string Texture => base.Texture + "arrowdown";
        public override int ArrowDust => DustID.BlueTorch;
    }
    public class Arrowup : ArrowBase
    {
        public override string Texture => base.Texture + "arrowup";
        public override int ArrowDust => DustID.GreenTorch;
    }
    public class Arrowright : ArrowBase
    {
        public override string Texture => base.Texture + "arrowright"; 
        public override int ArrowDust => DustID.Torch;
    }


}