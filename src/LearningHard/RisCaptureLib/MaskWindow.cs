﻿using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System;

namespace RisCaptureLib
{
    internal class MaskWindow : Window
    {
        private MaskCanvas innerCanvas;
        private Bitmap screenSnapshot;
        private Timer timeOutTimmer;
        private readonly ScreenCaputre screenCaputreOwner;

        //截图显示尺寸label
        System.Windows.Controls.Label label = null;
        //截图显示按键
        ToolBarControl toolBarContrl = null;
        //截图保存图片
        private Bitmap m_bmpLayerCurrent;
        //获取程序所在路径
        string filePath = AppDomain.CurrentDomain.BaseDirectory;

        public MaskWindow(ScreenCaputre screenCaputreOwner)
        {
            this.screenCaputreOwner = screenCaputreOwner;
            Ini();
            innerCanvas.OnMove += DrawShowSize;
        }

        private void Ini()
        {

            //ini normal properties
            //Topmost = true;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;

            //set bounds to cover all screens
            var rect = SystemInformation.VirtualScreen;
            Left = rect.X;
            Top = rect.Y;
            Width = rect.Width;
            Height = rect.Height;

            //set background 
            screenSnapshot = HelperMethods.GetScreenSnapshot();
            if (screenSnapshot != null)
            {
                var bmp = screenSnapshot.ToBitmapSource();
                bmp.Freeze();
                Background = new ImageBrush(bmp);
            }

            //ini canvas
            innerCanvas = new MaskCanvas
            {
                MaskWindowOwner = this
            };
            Content = innerCanvas;

        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            //鼠标右键双击取消
            if (e.RightButton == MouseButtonState.Pressed && e.ClickCount >= 2)
            {
                CancelCaputre();
            }

            CreatLabel(e.GetPosition(innerCanvas));
            label.Visibility = Visibility.Visible;
            if (toolBarContrl != null)
            {
                toolBarContrl.Visibility = Visibility.Hidden;

            }
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                base.OnMouseMove(e);
                if (timeOutTimmer != null && timeOutTimmer.Enabled)
                {
                    timeOutTimmer.Stop();
                    timeOutTimmer.Start();
                }
                //设置左上角label和右下角toolbar鼠标跟随
                Rect temRect = innerCanvas.GetSelectionRegion();
                label.Content = "Selected area size：" + temRect.Width + "×" + temRect.Height + "Wide：" + temRect.Width + "High：" + temRect.Height;
                Canvas.SetLeft(label, temRect.X);
                Canvas.SetTop(label, temRect.Y - 25);
            }
            catch (Exception)
            {

              
            }

            //Canvas.SetLeft(label, temRect.X + temRect.Width - 75);
            //Canvas.SetTop(label, temRect.Y + temRect.Height);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            CreatToolBar(e.GetPosition(innerCanvas));
            toolBarContrl.Visibility = Visibility.Visible;

        }

        //创建提示选中区域大小控件
        private void CreatLabel(System.Windows.Point location)
        {
            if (label == null)
            {
                label = new System.Windows.Controls.Label();
                innerCanvas.Children.Add(label);

            }
            label.Content = GetLabelContent();
            label.Height = 25;
            Canvas.SetLeft(label, location.X);
            Canvas.SetTop(label, location.Y - 25);
        }

        private void CreatToolBar(System.Windows.Point location)
        {
            //if (toolBarContrl == null)
            //{
            toolBarContrl = new ToolBarControl();
            innerCanvas.Children.Add(toolBarContrl);
            Canvas.SetLeft(toolBarContrl, location.X - 75);
            Canvas.SetTop(toolBarContrl, location.Y);
            //}
            toolBarContrl.OnOK += OKAction;
            toolBarContrl.OnCancel += CancelAction;
            toolBarContrl.OnSaveCapture += SaveCaptureAction;
        }
        private string GetLabelContent()
        {
            string strContent = "";
            return strContent;
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape)
            {
                CancelCaputre();
            }
        }

        private void CancelCaputre()
        {
            Close();
            screenCaputreOwner.OnScreenCaputreCancelled(null);
        }

        internal void OnShowMaskFinished(Rect maskRegion)
        {

        }

        internal void ClipSnapshot(Rect clipRegion)
        {
            BitmapSource caputredBmp = CopyFromScreenSnapshot(clipRegion);

            if (caputredBmp != null)
            {
                screenCaputreOwner.OnScreenCaputred(null, caputredBmp);
            }

            //close mask window
            Close();
        }


        internal BitmapSource CopyFromScreenSnapshot(Rect region)
        {
            var sourceRect = region.ToRectangle();
            var destRect = new Rectangle(0, 0, sourceRect.Width, sourceRect.Height);

            if (screenSnapshot != null)
            {
                var bitmap = new Bitmap(sourceRect.Width, sourceRect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(screenSnapshot, destRect, sourceRect, GraphicsUnit.Pixel);
                }

                return bitmap.ToBitmapSource();
            }

            return null;
        }

        //保存图片
        private void SaveCaptureAction()
        {
            m_bmpLayerCurrent = innerCanvas.GetSnapBitmap();
            if (m_bmpLayerCurrent == null)
            {
                return;
            }
            SaveFileDialog saveDlg = new SaveFileDialog();
            string mydocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            saveDlg.InitialDirectory = mydocPath/* + "\\"*/;
            saveDlg.Filter = "Bitmap(*.bmp)|*.bmp|JPEG(*.jpg)|*.jpg";
            saveDlg.FilterIndex = 2;

            saveDlg.FileName = DateTime.Now.Date.Millisecond.ToString();
            if (saveDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                switch (saveDlg.FilterIndex)
                {
                    case 1:
                        m_bmpLayerCurrent.Clone(new Rectangle(0, 0, m_bmpLayerCurrent.Width, m_bmpLayerCurrent.Height),
                            System.Drawing.Imaging.PixelFormat.Format24bppRgb).Save(saveDlg.FileName,
                            System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                    case 2:
                        m_bmpLayerCurrent.Save(saveDlg.FileName,
                            System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                }
            }

        }


        internal Bitmap CopyBitmapFromScreenSnapshot(Rect region)
        {
            var sourceRect = region.ToRectangle();
            var destRect = new Rectangle(0, 0, sourceRect.Width, sourceRect.Height);

            if (screenSnapshot != null)
            {
                var bitmap = new Bitmap(sourceRect.Width, sourceRect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(screenSnapshot, destRect, sourceRect, GraphicsUnit.Pixel);
                }

                return bitmap;
            }

            return null;
        }

        public void Show(int timeOutSecond, System.Windows.Size? defaultSize)
        {
            if (timeOutSecond > 0)
            {
                if (timeOutTimmer == null)
                {
                    timeOutTimmer = new Timer();
                    timeOutTimmer.Tick += OnTimeOutTimmerTick;
                }
                timeOutTimmer.Interval = timeOutSecond * 1000;
                timeOutTimmer.Start();
            }

            if (innerCanvas != null)
            {
                innerCanvas.DefaultSize = defaultSize;
            }

            Show();
            Focus();

        }

        private void OnTimeOutTimmerTick(object sender, EventArgs e)
        {
            timeOutTimmer.Stop();
            CancelCaputre();
        }

        public void DrawShowSize(Rect rec)
        {
            if (rec == Rect.Empty)
            {
                return;
            }
            var wX = rec.Width;
            var hY = rec.Height;
            label.Content = wX + "×" + hY + "Wide：" + wX + "High：" + hY;
           // CreatToolBar(rec);
        }

        private void CreatToolBar(Rect rect)
        {
            //if (toolBarContrl == null)
            //{
            toolBarContrl = new ToolBarControl();
            innerCanvas.Children.Add(toolBarContrl);
            Canvas.SetLeft(toolBarContrl, rect.X - 175);
            Canvas.SetTop(toolBarContrl, rect.Y);
            //}
            toolBarContrl.OnOK += OKAction;
            toolBarContrl.OnCancel += CancelAction;
            toolBarContrl.OnSaveCapture += SaveCaptureAction;
        }


        // 确定按钮事件
        public void OKAction()
        {

            innerCanvas.finishAction();

        }

        //保存图片
        private string SaveCaptureActionByOk()
        {
            try
            {
                m_bmpLayerCurrent = innerCanvas.GetSnapBitmap();
                if (m_bmpLayerCurrent == null)
                    return null;

                long t = GetTimestamp();
                filePath = filePath + t + ".png";
                m_bmpLayerCurrent.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                return filePath;
            }
            catch (Exception)
            {
                return null;
            }


        }

        // 时间戳
        protected long GetTimestamp()
        {

            TimeSpan cha = (DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)));
            long t = (long)cha.TotalMilliseconds;
            return t;
        }


        public void CancelAction()
        {
            CancelCaputre();
        }
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            label.Visibility = Visibility.Hidden;
            toolBarContrl.Visibility = Visibility.Hidden;
        }
    }
}
