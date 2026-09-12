using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace AulaTray;

public static class IconRenderer
{
    // Neon ışık renkleri
    private static readonly Color NeonGreen = Color.FromArgb(0, 245, 160);    // Parlak neon zümrüt
    private static readonly Color NeonYellow = Color.FromArgb(255, 205, 50);   // Neon sarı
    private static readonly Color NeonRed = Color.FromArgb(255, 75, 75);       // Neon kırmızı
    private static readonly Color ChargingCyan = Color.FromArgb(0, 220, 255);  // Şarj anında neon camgöbeği
    private static readonly Color KeycapWhite = Color.FromArgb(240, 245, 250);
    private static readonly Color FrameBorder = Color.FromArgb(200, 210, 225);
    private static readonly Color MutedGray = Color.FromArgb(120, 125, 135);

    public static Icon CreateBatteryIcon(KeyboardStatus status)
    {
        using Bitmap bitmap = new Bitmap(32, 32);
        using Graphics g = Graphics.FromImage(bitmap);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        g.Clear(Color.Transparent);

        // 1. Durum Rengi Belirleme
        Color barColor;
        if (status.ConnectionState != ConnectionState.Connected)
        {
            barColor = MutedGray;
        }
        else if (status.PowerState == PowerState.Charging)
        {
            barColor = ChargingCyan; // Şarjda camgöbeği neon parıltı
        }
        else
        {
            barColor = status.BatteryPercent switch
            {
                > 45 => NeonGreen,
                > 19 => NeonYellow,
                _ => NeonRed
            };
        }

        // 2. Klavye Gövdesi Çizimi (Aula F75 Silüeti)
        // x=2, y=5, w=28, h=17, radius=3
        float kX = 2f;
        float kY = 5f;
        float kW = 28f;
        float kH = 17f;

        // Klavye dış çerçevesi (Koyu içi hafif transparan, dışı beyaz hatlı)
        using (GraphicsPath framePath = CreateRoundedRectPath(kX, kY, kW, kH, 3f))
        {
            using Brush bgBrush = new SolidBrush(Color.FromArgb(160, 20, 24, 32));
            g.FillPath(bgBrush, framePath);

            using Pen borderPen = new Pen(status.ConnectionState == ConnectionState.Connected ? FrameBorder : MutedGray, 1.3f);
            g.DrawPath(borderPen, framePath);
        }

        // 3. Sağ Üstteki Meşhur Ses Tekeri (Knob)
        float knobX = kX + kW - 5.5f;
        float knobY = kY + 2f;
        float knobSize = 4.0f;
        using (Brush knobBrush = new SolidBrush(status.ConnectionState == ConnectionState.Connected ? Color.FromArgb(230, 235, 245) : MutedGray))
        {
            g.FillEllipse(knobBrush, knobX, knobY, knobSize, knobSize);
        }
        using (Pen knobBorder = new Pen(Color.FromArgb(100, 110, 130), 0.8f))
        {
            g.DrawEllipse(knobBorder, knobX, knobY, knobSize, knobSize);
        }

        // 4. Tuşlar veya Yüzde Metni
        if (status.ConnectionState == ConnectionState.Connected)
        {
            // Merkeze çok keskin, net ve sade yüzde metni
            string percentText = status.BatteryPercent.ToString();
            float fontSize = percentText.Length >= 3 ? 7.5f : 8.5f;
            using Font font = new Font("Segoe UI", fontSize, FontStyle.Bold);
            using Brush brush = new SolidBrush(KeycapWhite);

            SizeF textSize = g.MeasureString(percentText, font);
            float textX = kX + (kW - textSize.Width) / 2f - 1f; // knob'a çarpmaması için hafif sola
            float textY = kY + (kH - textSize.Height) / 2f;
            g.DrawString(percentText, font, brush, textX, textY);
        }
        else
        {
            // Bağlı değilse tire
            using Font font = new Font("Segoe UI", 8f, FontStyle.Bold);
            using Brush brush = new SolidBrush(MutedGray);
            g.DrawString("--", font, brush, kX + 8, kY + 2);
        }

        // 5. Klavyenin Altındaki Neon Pil / Şarj Işık Şeridi (Fotoğraftaki alt parıltı)
        float barY = kY + kH + 2f; // y=24
        float barH = 2.5f;

        if (status.ConnectionState == ConnectionState.Connected)
        {
            // Parlama efekti (Glow)
            using (Pen glowPen = new Pen(Color.FromArgb(90, barColor), 4.5f))
            {
                glowPen.StartCap = LineCap.Round;
                glowPen.EndCap = LineCap.Round;
                g.DrawLine(glowPen, kX + 3, barY + 1, kX + kW - 3, barY + 1);
            }

            // Doluluk oranına göre ana neon çizgi
            float fillRatio = Math.Clamp(status.BatteryPercent / 100f, 0.05f, 1.0f);
            float fillBarW = (kW - 4f) * fillRatio;

            using (GraphicsPath barPath = CreateRoundedRectPath(kX + 2, barY, fillBarW, barH, 1.2f))
            using (Brush barBrush = new SolidBrush(barColor))
            {
                g.FillPath(barBrush, barPath);
            }
        }
        else
        {
            // Sönük gri çizgi
            using (GraphicsPath barPath = CreateRoundedRectPath(kX + 2, barY, kW - 4, barH, 1.2f))
            using (Brush barBrush = new SolidBrush(MutedGray))
            {
                g.FillPath(barBrush, barPath);
            }
        }

        return Icon.FromHandle(bitmap.GetHicon());
    }

    private static GraphicsPath CreateRoundedRectPath(float x, float y, float w, float h, float r)
    {
        GraphicsPath path = new GraphicsPath();
        if (r <= 0) { path.AddRectangle(new RectangleF(x, y, w, h)); return path; }
        float d = r * 2;
        if (d > w) d = w;
        if (d > h) d = h;

        path.AddArc(x, y, d, d, 180, 90);
        path.AddArc(x + w - d, y, d, d, 270, 90);
        path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
        path.AddArc(x, y + h - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
