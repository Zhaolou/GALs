using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
namespace GALs
{
    public partial class Form1 : Form
    {
        public ScatteringProcessing scatteringProcessing = new ScatteringProcessing();
        public int planeWaveSpectrumShowType = 0; // = 0, Magnitude; = 1, Real; = 2, Image; = 3, Phase;
        public int dSCSShowType = 0; // = 0, Magnitude; = 1, Real; = 2, Image; 
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            chartPlaneWaveSpectrum.Titles.Add("Plane wave spectrum");
            comboBoxLightSheetType.Items.Add("Gaussian Light Sheet");
            comboBoxLightSheetType.Items.Add("Airy Light Sheet");
            comboBoxLightSheetType.Items.Add("Customized Plane Wave Spectrum");
            comboBoxLightSheetType.SelectedIndex = 0;
            groupBoxGaussian.Enabled = true;
            groupBoxAiry.Enabled = false;
            groupBoxCustomized.Enabled = false;
            comboBoxParticleType.Items.Add("Sphere");
            comboBoxParticleType.SelectedIndex = 0;
            comboBoxProcessingUnit.Items.Add("GPU");
            comboBoxProcessingUnit.Items.Add("CPU");
            comboBoxProcessingUnit.SelectedIndex = 1;
            textBoxWavelength.Text = "1";
            textBoxWaistRadius.Text = "1";
            textBoxTransverseScale.Text = "1";
            textBoxFiniteEnergyParameter.Text = "0.1";
            textBoxParticleRadius.Text = "50";
            textBoxRefractiveIndex.Text = "1.5";
            textBoxRefractiveIndexImage.Text = "0";
            textBoxXPosition.Text = "0";
            textBoxYPosition.Text = "0";
            comboBoxSeriesThreshold.Items.Add("0");
            comboBoxSeriesThreshold.SelectedIndex = 0;
            textBoxScatteringAngle.Text = "0:1:180";
            textBoxAngleStep.Text = "10001";
            UpdatePlaneWaveSpectrum();
        }

        private void comboBoxLightSheetType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxLightSheetType.SelectedIndex == 0)
            {
                groupBoxGaussian.Enabled = true;
                groupBoxAiry.Enabled = false;
                groupBoxCustomized.Enabled = false;
                scatteringProcessing.LightSheetType = 0;
            }
            else if (comboBoxLightSheetType.SelectedIndex == 1)
            {
                groupBoxGaussian.Enabled = false;
                groupBoxAiry.Enabled = true;
                groupBoxCustomized.Enabled = false;
                scatteringProcessing.LightSheetType = 1;
            }
            else
            {
                groupBoxGaussian.Enabled = false;
                groupBoxAiry.Enabled = false;
                groupBoxCustomized.Enabled = true;
                scatteringProcessing.LightSheetType = 2;
            }
            UpdatePlaneWaveSpectrum();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt)|*.txt|All files(*.*)|*>**";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            DialogResult dr = ofd.ShowDialog();
            if (dr == DialogResult.OK && ofd.FileName.Length > 0)
            {
                scatteringProcessing.ReadFile(ofd.FileName);
                textBoxFileName.Text = ofd.FileName;
                UpdatePlaneWaveSpectrum();
            }
        }

        public void UpdatePlaneWaveSpectrum()
        {
            int lightSheetType = comboBoxLightSheetType.SelectedIndex;
            chartPlaneWaveSpectrum.Series.Clear();

            for (int i = 0; i < 2001; i++)
            {
                scatteringProcessing.calculationParameter.spectrumNx[i] = (i - 1000) * 0.001;
                scatteringProcessing.calculationParameter.spectrumMag[i] = scatteringProcessing.GetAmplitudeAtFrequency((scatteringProcessing.calculationParameter.spectrumNx[i]), lightSheetType).Mod();
                scatteringProcessing.calculationParameter.spectrumReal[i] = scatteringProcessing.GetAmplitudeAtFrequency((scatteringProcessing.calculationParameter.spectrumNx[i]), lightSheetType).real;
                scatteringProcessing.calculationParameter.spectrumImage[i] = scatteringProcessing.GetAmplitudeAtFrequency((scatteringProcessing.calculationParameter.spectrumNx[i]), lightSheetType).image;
                scatteringProcessing.calculationParameter.spectrumPhase[i] = scatteringProcessing.GetAmplitudeAtFrequency((scatteringProcessing.calculationParameter.spectrumNx[i]), lightSheetType).GetAngle();
            }

            Series series0 = new Series();
            series0.ChartType = SeriesChartType.Line;
            series0.BorderWidth = 2;
            series0.Color = System.Drawing.Color.Red;
            series0.LegendText = "Magnitude";
            Series series1 = new Series();
            series1.ChartType = SeriesChartType.Line;
            series1.BorderWidth = 2;
            series1.Color = System.Drawing.Color.Blue;
            series1.LegendText = "Real";
            Series series2 = new Series();
            series2.ChartType = SeriesChartType.Line;
            series2.BorderWidth = 2;
            series2.Color = System.Drawing.Color.Green;
            series2.LegendText = "Image";
            Series series3 = new Series();
            series3.ChartType = SeriesChartType.Line;
            series3.BorderWidth = 2;
            series3.Color = System.Drawing.Color.Black;
            series3.LegendText = "Phase";
            chartPlaneWaveSpectrum.ChartAreas[0].AxisX.Title = "nx";
            if (planeWaveSpectrumShowType == 0)
                chartPlaneWaveSpectrum.ChartAreas[0].AxisY.Title = "Magnitude";
            else if (planeWaveSpectrumShowType == 1)
                chartPlaneWaveSpectrum.ChartAreas[0].AxisY.Title = "Real";
            else if (planeWaveSpectrumShowType == 2)
                chartPlaneWaveSpectrum.ChartAreas[0].AxisY.Title = "Image";
            else if (planeWaveSpectrumShowType == 3)
                chartPlaneWaveSpectrum.ChartAreas[0].AxisY.Title = "Phase";
            for (int i = 0; i < 2001; i++)
            {
                series0.Points.AddXY(scatteringProcessing.calculationParameter.spectrumNx[i], scatteringProcessing.calculationParameter.spectrumMag[i]);
                series1.Points.AddXY(scatteringProcessing.calculationParameter.spectrumNx[i], scatteringProcessing.calculationParameter.spectrumReal[i]);
                series2.Points.AddXY(scatteringProcessing.calculationParameter.spectrumNx[i], scatteringProcessing.calculationParameter.spectrumImage[i]);
                series3.Points.AddXY(scatteringProcessing.calculationParameter.spectrumNx[i], scatteringProcessing.calculationParameter.spectrumPhase[i]);
            }
            if (planeWaveSpectrumShowType == 0)
            {
                chartPlaneWaveSpectrum.Series.Add(series0);
                chartPlaneWaveSpectrum.ChartAreas[0].AxisY.Maximum = Math.Round(scatteringProcessing.calculationParameter.spectrumMag.Max(), 1) + 0.1;
                chartPlaneWaveSpectrum.ChartAreas[0].AxisY.Minimum = Math.Round(scatteringProcessing.calculationParameter.spectrumMag.Min(), 1) - 0.1;
            }
            else if (planeWaveSpectrumShowType == 1)
            {
                chartPlaneWaveSpectrum.Series.Add(series1);
                chartPlaneWaveSpectrum.ChartAreas[0].AxisY.Maximum = Math.Round(scatteringProcessing.calculationParameter.spectrumReal.Max(), 1) + 0.1;
                chartPlaneWaveSpectrum.ChartAreas[0].AxisY.Minimum = Math.Round(scatteringProcessing.calculationParameter.spectrumReal.Min(), 1) - 0.1;
            }
            else if (planeWaveSpectrumShowType == 2)
            {
                chartPlaneWaveSpectrum.Series.Add(series2);
                chartPlaneWaveSpectrum.ChartAreas[0].AxisY.Maximum = Math.Round(scatteringProcessing.calculationParameter.spectrumImage.Max(), 1) + 0.1;
                chartPlaneWaveSpectrum.ChartAreas[0].AxisY.Minimum = Math.Round(scatteringProcessing.calculationParameter.spectrumImage.Min(), 1) - 0.1;
            }
            else if (planeWaveSpectrumShowType == 3)
            {
                chartPlaneWaveSpectrum.Series.Add(series3);
                chartPlaneWaveSpectrum.ChartAreas[0].AxisY.Maximum = Math.Round(scatteringProcessing.calculationParameter.spectrumPhase.Max(), 1) + 0.1;
                chartPlaneWaveSpectrum.ChartAreas[0].AxisY.Minimum = Math.Round(scatteringProcessing.calculationParameter.spectrumPhase.Min(), 1) - 0.1;
            }
        }
        private void chartPlaneWaveSpectrum_Click(object sender, EventArgs e)
        {
            UpdatePlaneWaveSpectrum();            
        }

        private void magnitudeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            magnitudeToolStripMenuItem.CheckState = CheckState.Checked;
            realComponentToolStripMenuItem.CheckState = CheckState.Unchecked;
            imageComponentToolStripMenuItem.CheckState = CheckState.Unchecked;
            phaseToolStripMenuItem.CheckState = CheckState.Unchecked;
            planeWaveSpectrumShowType = 0;
            UpdatePlaneWaveSpectrum();
        }

        private void realComponentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            magnitudeToolStripMenuItem.CheckState = CheckState.Unchecked;
            realComponentToolStripMenuItem.CheckState = CheckState.Checked;
            imageComponentToolStripMenuItem.CheckState = CheckState.Unchecked;
            phaseToolStripMenuItem.CheckState = CheckState.Unchecked;
            planeWaveSpectrumShowType = 1;
            UpdatePlaneWaveSpectrum();

        }

        private void imageComponentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            magnitudeToolStripMenuItem.CheckState = CheckState.Unchecked;
            realComponentToolStripMenuItem.CheckState = CheckState.Unchecked;
            imageComponentToolStripMenuItem.CheckState = CheckState.Checked;
            phaseToolStripMenuItem.CheckState = CheckState.Unchecked;
            planeWaveSpectrumShowType = 2;
            UpdatePlaneWaveSpectrum();
        }

        private void phaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            magnitudeToolStripMenuItem.CheckState = CheckState.Unchecked;
            realComponentToolStripMenuItem.CheckState = CheckState.Unchecked;
            imageComponentToolStripMenuItem.CheckState = CheckState.Unchecked;
            phaseToolStripMenuItem.CheckState = CheckState.Checked;
            planeWaveSpectrumShowType = 3;
            UpdatePlaneWaveSpectrum();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int lightSheetType = comboBoxLightSheetType.SelectedIndex;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "ext files (*.txt)|*.txt|All files(*.*)|*>**";
            sfd.FilterIndex = 1;
            sfd.RestoreDirectory = true;
            DialogResult dr = sfd.ShowDialog();
            if (dr == DialogResult.OK && sfd.FileName.Length > 0)
            {
                FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                string str;
                
                double[] nx = new double[2001];
                double[] dataTempReal = new double[2001];
                double[] dataTempImage = new double[2001];
                for (int i = 0; i < 2001; i++)
                {
                    nx[i] = (i - 1000) * 0.001;
                    dataTempReal[i] = scatteringProcessing.GetAmplitudeAtFrequency(nx[i], lightSheetType).real;
                    dataTempImage[i] = scatteringProcessing.GetAmplitudeAtFrequency(nx[i], lightSheetType).image;
                    str = nx[i].ToString("f4") + "    " + dataTempReal[i].ToString("f6") + "    " + dataTempImage[i].ToString("f6");
                    sw.WriteLine(str);
                }
                sw.Close();
                fs.Close();
            }
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            UpdatePlaneWaveSpectrum();
        }

        private void textBoxWaistRadius_TextChanged(object sender, EventArgs e)
        {
            string str = textBoxWaistRadius.Text;
            try
            {
                double t = Convert.ToDouble(str);
                scatteringProcessing.gl.waistRadius = t * 1e-6;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void textBoxTransverseScale_TextChanged(object sender, EventArgs e)
        {
            string str = textBoxTransverseScale.Text;
            try
            {
                double t = Convert.ToDouble(str);
                scatteringProcessing.al.xr = t * 1e-6;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void textBoxFiniteEnergyParameter_TextChanged(object sender, EventArgs e)
        {
            string str = textBoxFiniteEnergyParameter.Text;
            try
            {
                double t = Convert.ToDouble(str);
                scatteringProcessing.al.a0 = t;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void textBoxWavelength_TextChanged(object sender, EventArgs e)
        {
            string str = textBoxWavelength.Text;
            try
            {
                double t = Convert.ToDouble(str);
                scatteringProcessing.al.wavelength = t * 1e-6;
                scatteringProcessing.gl.wavelength = t * 1e-6;
                scatteringProcessing.cl.wavelength = t * 1e-6;
                scatteringProcessing.wavelength = t * 1e-6;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public double deg2rad(double s)
        {
            return s * Math.PI / 180;
        }
        public bool UpdateParameter()
        {
            double ra, rir, rii, xp, yp;
            try
            {
                ra = Convert.ToDouble(textBoxParticleRadius.Text) * 1e-6;
                rir = Convert.ToDouble(textBoxRefractiveIndex.Text);
                rii = Convert.ToDouble(textBoxRefractiveIndexImage.Text);
                xp = Convert.ToDouble(textBoxXPosition.Text) * 1e-6;
                yp = Convert.ToDouble(textBoxYPosition.Text) * 1e-6;
                scatteringProcessing.sphereParameter.Copy(ra, rir, rii, xp, yp);
                scatteringProcessing.calculationParameter.seriesThreshold = 0;
                scatteringProcessing.calculationParameter.angleStep = 10001;
                scatteringProcessing.calculationParameter.scatteringAngle.Clear();
                scatteringProcessing.calculationParameter.processingUnitType = comboBoxProcessingUnit.SelectedIndex;
                string s = textBoxScatteringAngle.Text;
                if (s.Contains(":"))
                {
                    string[] sA = s.Split(':');
                    if (sA.Length != 3)
                    {
                        MessageBox.Show("The format of scattering angle is wrong.");
                        return false;
                    }
                    double start = Convert.ToDouble(sA[0]);
                    double step = Convert.ToDouble(sA[1]);
                    double end = Convert.ToDouble(sA[2]);

                    while (start <= end)
                    {
                        scatteringProcessing.calculationParameter.scatteringAngle.Add(deg2rad(start));
                        start = start + step;
                    }
                }
                else
                {
                    string[] sA = s.Split(new char[2] { ' ', ',' }, options: StringSplitOptions.RemoveEmptyEntries);
                    foreach (string st in sA)
                    {
                        scatteringProcessing.calculationParameter.scatteringAngle.Add(deg2rad(Convert.ToDouble(st)));
                    }
                }
                scatteringProcessing.calculationParameter.lightsheetScatteringAmplitude.Clear();
                for (int i = 0; i < scatteringProcessing.calculationParameter.scatteringAngle.Count; i++)
                {
                    scatteringProcessing.calculationParameter.lightsheetScatteringAmplitude.Add(new Complex(0, 0));
                }

                try
                {
                    scatteringProcessing.calculationParameter.seriesThreshold = (int)Convert.ToDouble(comboBoxSeriesThreshold.Text);
                    scatteringProcessing.calculationParameter.angleStep = (int)Convert.ToDouble(textBoxAngleStep.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            return true;
        }
        public void UpdateDSCS()
        {
            chartDSCS.Series.Clear();
            chartDSCS.ChartAreas[0].AxisX.LabelStyle.Format = "N1";
            chartDSCS.ChartAreas[0].AxisY.LabelStyle.Format = "N1";
            Series series0 = new Series();
            series0.ChartType = SeriesChartType.Line;
            series0.BorderWidth = 2;
            series0.Color = System.Drawing.Color.Red;
            series0.LegendText = "Normalized DSCS";
            chartDSCS.ChartAreas[0].AxisX.Title = "Scattering angle";
            chartDSCS.ChartAreas[0].AxisY.Title = "DSCS/lambda (dB)";
            double maxMag = double.MinValue;
            double minMag = double.MaxValue;

            for (int i = 0; i < scatteringProcessing.calculationParameter.scatteringAngle.Count; i++)
            {
                maxMag = Math.Max(maxMag, 10 * Math.Log10(Math.Pow(scatteringProcessing.calculationParameter.lightsheetScatteringAmplitude[i].Mod(), 2) / Math.PI));
                minMag = Math.Min(minMag, 10 * Math.Log10(Math.Pow(scatteringProcessing.calculationParameter.lightsheetScatteringAmplitude[i].Mod(), 2) / Math.PI));

            }

            for (int i = 0; i < scatteringProcessing.calculationParameter.scatteringAngle.Count; i++)
            {
                series0.Points.AddXY(scatteringProcessing.calculationParameter.scatteringAngle[i], 10 * Math.Log10(Math.Pow(scatteringProcessing.calculationParameter.lightsheetScatteringAmplitude[i].Mod(), 2) / Math.PI));
            }
                chartDSCS.Series.Add(series0);
                chartDSCS.ChartAreas[0].AxisY.Maximum = maxMag + 0.1;
                chartDSCS.ChartAreas[0].AxisY.Minimum = minMag - 0.1;
            
        }
        private void buttonRun_Click(object sender, EventArgs e)
        {
            if(UpdateParameter() == true)
                scatteringProcessing.RunSimulation(scatteringProcessing.calculationParameter.processingUnitType);
            UpdateDSCS();
        }

        private void comboBoxProcessingUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            scatteringProcessing.calculationParameter.processingUnitType = comboBoxProcessingUnit.SelectedIndex;
        }

        private void saveDSCSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "ext files (*.txt)|*.txt|All files(*.*)|*>**";
            sfd.FilterIndex = 1;
            sfd.RestoreDirectory = true;
            double d; 
            DialogResult dr = sfd.ShowDialog();
            if (dr == DialogResult.OK && sfd.FileName.Length > 0)
            {
                FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                string str;
                for (int i = 0; i < scatteringProcessing.calculationParameter.scatteringAngle.Count; i++)
                {
                    d = 10 * Math.Log10(Math.Pow(scatteringProcessing.calculationParameter.lightsheetScatteringAmplitude[i].Mod(), 2) / Math.PI);
                    str = d.ToString("f6");
                    sw.WriteLine(str);
                }
                sw.Close();
                fs.Close();
            }
        }


    }
}
