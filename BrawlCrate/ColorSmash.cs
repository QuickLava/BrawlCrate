﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BrawlCrate.NodeWrappers;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlCrate
{
    public class ColorSmash
    {
        public static void ColorSmashTex0(object sender, EventArgs e)
        {
            DirectoryInfo inputDir = Directory.CreateDirectory(Program.AppPath + "\\cs\\");
            DirectoryInfo outputDir = Directory.CreateDirectory(Program.AppPath + "\\cs\\out\\");
            try
            {
                foreach (FileInfo f in outputDir.GetFiles())
                {
                    try
                    {
                        f.Delete();
                    }
                    catch
                    {

                    }
                }
                foreach (FileInfo f in inputDir.GetFiles())
                {
                    try
                    {
                        f.Delete();
                    }
                    catch
                    {

                    }
                }

                if (MainForm.Instance.resourceTree.SelectedNodes.Count <= 1)
                {
                    return;
                }

                if (inputDir.GetFiles().Length > 0 || outputDir.GetFiles().Length > 0)
                {
                    MessageBox.Show(
                        "One or more files exist in the required color smash folder. Please delete these nodes manually and try again",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                short paletteCount = 0;
                List<TEX0Node> textureList = new List<TEX0Node>();
                Dictionary<int, string> names = new Dictionary<int, string>();
                BRRESNode b = (((TEX0Wrapper)MainForm.Instance.resourceTree.SelectedNodes[0]).Resource as TEX0Node)?
                    .BRESNode;
                foreach (TreeNode n in MainForm.Instance.resourceTree.SelectedNodes)
                {
                    if (!(n is TEX0Wrapper))
                    {
                        return;
                    }

                    if (((TEX0Wrapper)n).Resource is TEX0Node t)
                    {
                        if (t.BRESNode != b)
                        {
                            MessageBox.Show("Color Smash is only supported for nodes in the same BRRES", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        textureList.Add(t);
                        int placement = t.Parent.Children.IndexOf(t);
                        names.Add(placement, t.Name);
                        t.Export($"{inputDir.FullName}\\{placement:D5}.png");
                        if (t.HasPalette && t.GetPaletteNode() != null &&
                            t.GetPaletteNode().Palette.Entries.Length > paletteCount)
                        {
                            paletteCount = (short)Math.Min(t.GetPaletteNode().Palette.Entries.Length, 256);
                        }
                    }
                }

                if (paletteCount == 0)
                {
                    paletteCount = 256;
                }

                int index = textureList[0].Parent.Children.IndexOf(textureList[0]);

                foreach (TEX0Node t in textureList)
                {
                    if (t.HasPalette && t.GetPaletteNode() != null)
                    {
                        t.GetPaletteNode().Remove();
                    }
                    t.Remove();
                }

                Process cSmash = Process.Start(new ProcessStartInfo()
                {
                    FileName = Program.AppPath + "\\color_smash.exe",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = $"-c RGB5A3 -n {paletteCount}",
                });
                cSmash?.WaitForExit();

                textureList = new List<TEX0Node>();
                int count = 0;
                BRESGroupNode texGroup = b.GetOrCreateFolder<TEX0Node>();
                foreach (FileInfo f in outputDir.GetFiles())
                {
                    FileInfo f2 = new FileInfo($"{inputDir.FullName}\\{f.Name}");
                    int i = int.Parse(f.Name.Substring(0, f.Name.IndexOf(".", StringComparison.Ordinal)));
                    using (TextureConverterDialog dlg = new TextureConverterDialog())
                    {
                        dlg.ImageSource = f.FullName;
                        dlg.ChkImportPalette.Checked = true;
                        dlg.Automatic = true;
                        if (dlg.ShowDialog(MainForm.Instance, b) != DialogResult.OK)
                        {
                            continue;
                        }

                        TEX0Node t = dlg.TEX0TextureNode;
                        t.Name = names[i];
                        textureList.Add(t);
                        dlg.Dispose();
                        t.OriginalPath = "";
                        count++;
                    }
                    f2.Delete();
                }

                textureList.Remove(textureList.Last());
                foreach (TEX0Node t in textureList)
                {
                    t.SharesData = true;
                }

                if (inputDir.GetFiles().Length > 0)
                {
                    MessageBox.Show(
                        "One or more files threw an error. Please ensure all relevant textures can be Color Smashed together.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    foreach (FileInfo f in inputDir.GetFiles())
                    {
                        int i = int.Parse(f.Name.Substring(0, f.Name.IndexOf(".", StringComparison.Ordinal)));
                        using (TextureConverterDialog dlg = new TextureConverterDialog())
                        {
                            dlg.ImageSource = f.FullName;
                            dlg.ChkImportPalette.Checked = true;
                            dlg.Automatic = true;
                            if (dlg.ShowDialog(MainForm.Instance, b) != DialogResult.OK)
                            {
                                continue;
                            }

                            TEX0Node t = dlg.TEX0TextureNode;
                            t.Name = names[i];

                            dlg.Dispose();
                            t.OriginalPath = "";
                            count++;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                foreach (FileInfo f in outputDir.GetFiles())
                {
                    try
                    {
                        f.Delete();
                    }
                    catch
                    {

                    }
                }
                try
                {
                    outputDir.Delete();
                }
                catch
                {

                }
                foreach (FileInfo f in inputDir.GetFiles())
                {
                    try
                    {
                        f.Delete();
                    }
                    catch
                    {

                    }
                }
                try
                {
                    inputDir.Delete();
                }
                catch
                {

                }
            }
        }
    }
}