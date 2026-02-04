using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using static System.Net.WebRequestMethods;
using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using WiCAM.Pn4000.Common;
using Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Windows.Media;

//using System.Runtime.InteropServices.WindowsRuntime;
using File = System.IO.File;
using WiCAM.Pn4000.TechnoTable;
using WiCAM.Pn4000.ScreenD3D.Renderer;
using WiCAM.Pn4000.BendModel;
using AnyCAD.Platform;
using SceneNode = AnyCAD.Foundation.SceneNode;
using Vector3 = AnyCAD.Foundation.Vector3;
using System.Drawing;
using AnyCAD.Foundation;
using System.Text.Json;
using AnyCAD.Exchange;
using SharpDX.Direct3D11;
using WiCAM.Pn4000.BendModel.GeometryGenerators;
using WiCAM.Pn4000.ScreenD3D;
using System.Windows.Interop;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;
using WiCAM.Pn4000.PN3D;
using System.Collections.ObjectModel;
using WiCAM.Pn4000.BendModel.Base.Graph;
using TreeView = System.Windows.Controls.TreeView;
using TopoExplor = AnyCAD.Foundation.TopoExplor;
using WiCAM.Pn4000.PN3D.Enums;
using Document = AnyCAD.Foundation.Document;
using ControlzEx.Standard;
using WiCAM.Pn4000.PN3D.Doc;
using static System.Net.Mime.MediaTypeNames;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.WpfControls.PpStatusWindow;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Archive.Browser.Classes.Geo3D;
using System.Windows.Media.Media3D;
using WiCAM.Pn4000.BendModel.GeometryTools;
using Matrix4d = WiCAM.Pn4000.BendModel.Base.Matrix4d;
using WiCAM.Pn4000.ScreenD3D.Renderer.RenderTasks;
using WiCAM.Pn4000.BendModel.Config;
using WiCAM.Pn4000.PN3D.Assembly.DisassemblyWindowHelpers;
using Vector3d = WiCAM.Pn4000.BendModel.Base.Vector3d;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using Model = WiCAM.Pn4000.BendModel.Model;
using WiCAM.Pn4000.BendDoc;
using Microsoft.Extensions.DependencyInjection;
using Application = System.Windows.Application;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Pn4000.Screen;
using WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes;
using WiCAM.Services.ConfigProviders.Contracts;
using Image = System.Windows.Controls.Image;
using KeyEventHandler = System.Windows.Input.KeyEventHandler;
using WiCAM.Pn4000.pn4.pn4FlowCenter;


namespace WiCAM.Pn4000.JobManager
{
    public partial class PartEditControl : UserControl, IView, IComponentConnector
    {
        public ExeFlow ExeFlow { get; private set; }

        public static PartEditControl _PartEditControl;
        private bool traced;
        public string pathToPNdrive;
        public string pathToFPR996 = "\\u\\pn\\pfiles\\00\\FPR996";
        public string pathToFPR997 = "\\u\\pn\\pfiles\\00\\FPR997";
        public string pathToUserPartSource = "C:\\pnusers\\default\\Quelle";
        public string pathToFPR999 = "\\u\\pn\\pfiles\\00\\FPR999";
        string _MousePosition = string.Empty;
        private BendModel.Model _floor;
        private bool floorHere;
        private WiCAM.Pn4000.BendModel.Model _clickSphere;
        private FeatureContext featureContext;

        Dictionary<string, Vector3> mFeatureColorTable = new Dictionary<string, Vector3>();
        private BrepSceneNode node;
        private BrepSceneNode wireNode;
        private Screen3D Geometry3dViewer;
        private Face startFace;

        public BendModel.Base.Vector3d RotationPoint { get; set; }
        public IFactorio _factorio;

        public PartEditControl()
        {
            _factorio = MainWindow.Instance._factorio;
            this.InitializeComponent();
            _PartEditControl = this;

            mFeatureColorTable.Add("Outter", ColorTable.LightSeaGreen);
            mFeatureColorTable.Add("Inner", ColorTable.LightSkyBlue);
            mFeatureColorTable.Add("Hole", ColorTable.LightCoral);

          
            //  pathToPNdrive = PnPathBuilder.PnDrive;
            //  CheckToolBar();
            ReadFPRtoEditBox();

            Geo3DInfo geo3DInfo = new Geo3DInfo();
            string geopfad = "P:\\u\\ar\\ar0007\\n3d\\190101_001.c3do";
            Viewport3D viewport3D = new Viewport3D();

       

            /*    editor.Foreground = System.Windows.Media.Brushes.White;
                PlaneModel planeModel = new PlaneModel(new WiCAM.Pn4000.BendModel.Base.Vector3d(0.0, 0.0, 0.0), new WiCAM.Pn4000.BendModel.Base.Vector3d(1.0, 0.0, 0.0), new WiCAM.Pn4000.BendModel.Base.Vector3d(0.0, 0.0, 1.0), 10000.0, 1, new WiCAM.Pn4000.BendModel.Base.Color(0.5f, 0.5f, 0.5f, 1f));
                this._floor = (WiCAM.Pn4000.BendModel.Model)planeModel;
                _ = xScreen3D.Navigate3DView.Visibility = Visibility.Visible;
                ScreenD3D11 screenD3D11 = new ScreenD3D11(new IntPtr(),null,200,200,xScreen3D);
                screenD3D11.AddBillboard(new Texture2DBillboard());
                screenD3D11.AddModel(_floor);
                RenderData renderData = new RenderData(new IntPtr(), 200,200,screenD3D11.Renderer);
                screenD3D11.Render(true);
                WiCAM.Pn4000.BendModel.Model model = new BendModel.Model();
                double a1 = 100.0;
                double a2 = 100.0;
                if (!double.IsNaN(this.Width))
                    a1 = this.Width;
                if (!double.IsNaN(this.Height))
                    a2 = this.Height;
                screenD3D11 = new ScreenD3D11(new WindowInteropHelper(MainWindow._instance).Handle, xScreen3D.InteropImage, (int)Math.Ceiling(a1), (int)Math.Ceiling(a2), xScreen3D);
              //  this.DebugInfo.ScreenD3D = this.ScreenD3D;
                xScreen3D.Navigate3DView.DataContext = (object)new Navigate3DViewModel(xScreen3D, screenD3D11);
              //  this.ScreenInfoView.DataContext = (object)new ScreenInfoViewModel();
                double num = (double)screenD3D11.Renderer.Zoom * 0.01;
                if (this._clickSphere != null)
                    this._clickSphere.Shells.First<Shell>().Transform = WiCAM.Pn4000.BendModel.Base.Matrix4d.Scale(num, num, num);
                screenD3D11.AddModel(this._clickSphere);
                //  xScreen3D.ScreenD3D.Renderer.Init();//.AddBillboard(new Texture2DBillboard());
                //          this.xScreen3D.ScreenD3D.AddModel(null);
                this.floorHere = true;
            */


            PlaneModel planeModel = new PlaneModel(new WiCAM.Pn4000.BendModel.Base.Vector3d(0.0, 0.0, 0.0), new WiCAM.Pn4000.BendModel.Base.Vector3d(1.0, 0.0, 0.0), new WiCAM.Pn4000.BendModel.Base.Vector3d(0.0, 0.0, 1.0), 10000.0, 1, new WiCAM.Pn4000.BendModel.Base.Color(0.5f, 0.5f, 0.5f, 1f));
            this._floor = (WiCAM.Pn4000.BendModel.Model)planeModel;

           // _ = xScreen3D.Navigate3DView.Visibility = Visibility.Visible;
        //    xScreen3D.Navigate3DView.RotationBtn.MouseLeftButtonDown += RotationBtn_MouseLeftButtonDown;
        //    Console.WriteLine(xScreen3D.Navigate3DView.RotationBtn.Name);
         //   TransparentCircleSectionButton transparentCircleSectionButton = xScreen3D.Navigate3DView.RotationBtn;
            double a1 = 100.0;
            double a2 = 100.0;
            if (!double.IsNaN(this.Width))
                a1 = this.Width;
            if (!double.IsNaN(this.Height))
                a2 = this.Height;
       //     ScreenD3D11 screenD3D11 = new ScreenD3D11(new WindowInteropHelper(MainWindow._instance).Handle, xScreen3D.InteropImage, (int)Math.Ceiling(a1), (int)Math.Ceiling(a2), xScreen3D);

           // screenD3D11 = new ScreenD3D11(new IntPtr(), null, 200, 200, xScreen3D);
      //      screenD3D11.AddBillboard(new Texture2DBillboard());
          
       //     xScreen3D.Navigate3DView.DataContext = (object)new Navigate3DViewModel(xScreen3D, screenD3D11);
       //       xScreen3D.ScreenInfoView.DataContext = (object)new ScreenInfoViewModel();
       //     double num = (double)screenD3D11.Renderer.Zoom * 0.01;
            //if (this._clickSphere != null)
               // this._clickSphere.Shells.First<Shell>().Transform = WiCAM.Pn4000.BendModel.Base.Matrix4d.Scale(num, num, num);
       //     screenD3D11.AddModel(_clickSphere);
      //      screenD3D11.AddModel(_floor);
      //      screenD3D11.PrintScreen();
       //     xScreen3D.Background = System.Windows.Media.Brushes.Aqua;
       //     xScreen3D.ScreenD3D = screenD3D11;
       //     RenderData rdata =new RenderData(new IntPtr(),400,400, xScreen3D.ScreenD3D.Renderer);
        //    ScreenD3D.Renderer.Renderer renderer = new ScreenD3D.Renderer.Renderer(new IntPtr(), 400, 400, xScreen3D);
            //   xScreen3D.ScreenD3D.Renderer.Init();//.AddBillboard(new Texture2DBillboard());
        //    xScreen3D.ScreenD3D.AddModel(_floor);
         //   xScreen3D.ScreenD3D.Render(true);
            //    xScreen3D.ScreenD3D.PrintScreen();
          //  xScreen3D.ShowNavigation(true);
            string text = "P:\\u\\ar\\ar0007\\n3d\\9X000FA6105R0-004.c3do";
            BendModel.Model model2 = new BendDoc.DocSerializer(null, null).DeserializeGeometry(text);
            Pair<BendModel.Base.Vector3d, BendModel.Base.Vector3d> pair = null;
            pair = model2.GetBoundary(Matrix4d.Identity);
            model2.ModelType = ModelType.Part;
            model2.PartRole = PartRole.BendModel;
            model2.Enabled = true;
            model2.ScreenSize = 1.0;
            Kernel3DScreen = new Screen3D
            {
                SnapsToDevicePixels = true
            };
            Kernel3DScreen.Loaded += ViewerLoaded;
          //  ContainerGeometry3dViewer.Content = Geometry3dViewer;
            _clickSphere = new BendModel.GeometryGenerators.Sphere(RotationPoint, 0.5, 25, 25, new WiCAM.Pn4000.BendModel.Base.Color(0f, 1f, 0f, 1f))
            {
                Enabled = false,
                ModelType = ModelType.System,
                PartRole = PartRole.ClickSphere,
                ScreenSize = 0.008
            };

          //  Geometry3dViewer.ScreenD3D = 

   //         Geometry3dViewer.ScreenD3D.AddModel(_clickSphere);
        //    xScreen3D.ScreenD3D?.AddModel(model2);
         //   xScreen3D.ScreenD3D.Render(skipQueuedFrames: true);
       //     xScreen3D.ScreenD3D.ZoomExtend();
          //  xScreen3D.ScreenD3D.AddModel(model2, true, new Action<RenderTaskResult>(ScreenRenderResult));
        //    xScreen3D.ScreenD3D.ProjectionType = ProjectionType.Perspective;
       //     xScreen3D.ScreenD3D.ZoomExtend();
         //   xScreen3D.ScreenD3D.Render(skipQueuedFrames: true);
            int target;
            Face firstFace;
            Console.WriteLine(model2.CreateUnfoldModel(model2.PartInfo, null, null, out firstFace));
            Console.WriteLine(firstFace.ID);
         //   Console.WriteLine(model2.GetAllFaces().Count);
            Shell shell = model2.Shell;
            Model model3 = new Model()
            {
                BodyId = model2.BodyId,
                PartId = model2.PartId,
                PartRole = model2.PartRole,
                ModelType = model2.ModelType,
                Enabled = true,
                ScreenSize = 1.0
            };
               

            Dictionary<FaceGroup, FaceGroup> copied = new Dictionary<FaceGroup, FaceGroup>();
            startFace = null;
            if (shell.Macros.FirstOrDefault((Macro x) => x is Deepening deepening2 && deepening2.IsSpecialVisible) is Deepening deepening)
            {
                Face face = deepening.Faces.First();
                FaceGroup faceGroup = face.FaceGroup;
                HashSet<Face> hashSet = null;
                if (faceGroup.Side0.Contains(face))
                {
                    hashSet = faceGroup.Side0;
                    Console.WriteLine("FaceGroup Side0 contains face: " + faceGroup.Side0.Count);
                }
                else if (faceGroup.Side1.Contains(face))
                {
                    hashSet = faceGroup.Side1;
                    Console.WriteLine("FaceGroup Side0 contains face: " + faceGroup.Side1.Count);

                }

                startFace = hashSet?.Where((Face f) => f.Macro == null)?.MaxBy((Face f) => f.Area);
                Console.WriteLine("FaceGroup Side0 contains face: " + startFace);
            }

            if (startFace == null)
            {
                startFace = shell.FlatFaceGroups.SelectMany((FaceGroup g) => g.Side0.Concat(g.Side1).Concat(g.SubGroups.SelectMany((FaceGroup sg) => sg.Side0.Concat(sg.Side1)))).MaxBy((Face f) => f.Area);
                Console.WriteLine("FaceGroup Side0 contains face: " + startFace);

            }

            if (startFace == null)
            {
                startFace = shell.Faces.MaxBy((Face f) => f.Area);
                Console.WriteLine("FaceGroup Side0 contains face: " + startFace);

            }
            geo3DInfo.LoadFromP3D(geopfad);
            geo3DInfo.SetModel3DOutput(viewport3D);
          //  ContainerGeometry3dViewer.Content = viewport3D;
            viewport3D.MinHeight = 600;
            viewport3D.MinWidth = 1200;

            Model model4;

       //    ExeFlow.LocalOpen3DPart_XWindowEdition(stepFile);
         ////   if (model2.Shells == null || model2.Shells.Count == 0 || model2.GetAllFaces().Count == 0)
         //   {
         //      Console.WriteLine(model2.GetAllFaces().Count);
         //  Console.WriteLine("Das Modell enthält keine Geometrie und kann nicht angezeigt werden.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
         //  return;
         //  }
         //  else
         //    {
         //     Console.WriteLine(model2.GetAllFaces().Count);
         //      (Triangle trimod,  model4) = model2.GetTriangleModelById(model2.BodyId);

            //     }
            //    Console.WriteLine(model4.GetAllFaces().Count);
            //      Border3D.Child = xScreen3D;
            //      xScreen3D.ScreenD3D.AddModel(model4, true, new Action<RenderTaskResult>(ScreenRenderResult));
            //   xScreen3D.ScreenD3D.AddModel(model2, true, new Action<RenderTaskResult>(ScreenRenderResult));
            // xScreen3D.ScreenD3D.ProjectionType = ProjectionType.Perspective;
            //   xScreen3D.ScreenD3D.ZoomExtend();
            //      xScreen3D.ScreenD3D.Render(skipQueuedFrames: true);
            //geo3DInfo.modelVisual3D.Children.Add(pair as Visual3D);
            //      xScreen3D.ShowClickSphere(_floor.SpecialCharacteristicPoints.First());
        }
        const string stepFile = @"C:\Users\tommy\Desktop\stp\1447-101-WP-0001-01_pT_00.stp";

        private async Task Initialize3DViewAsync()
        {
            const string c3dFile = @"C:\u\ar\ar0007\n3d\WBLG_8001.c3do";
            Kernel3DScreen.ShowNavigation(true);
            //var spatial = SpatialStarter.GetSpatialStartInfoForImport(c3dFile, null, PnPathBuilder.PnDrive, "C:\\", true);
           // spatial.ArgumentList.Add("-I");
           // spatial.ArgumentList.Add(c3dFile);
          //var run =  Process.Start(spatial);
          /*  if (run == null)
            {
                Console.WriteLine("Fehler beim Starten des Spatial Importers.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            await run.WaitForExitAsync();
           // var partAnalyzer = new PartAnalyzer();
         //   var spatialLoader = new SpatialLoader(partAnalyzer);
            var analyzeConfig = new AnalyzeConfig();
          */
            BendModel.Model model;
            try
            {
          //      model = await Task.Run(() => spatialLoader.LoadSpatialFile(stepFile, System.IO.Path.GetFileNameWithoutExtension(stepFile), false, analyzeConfig));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden der STEP-Datei: {ex.Message}", "Ladefehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
         /*   model.BodyId = model.PartId;
            Console.WriteLine(model.BodyId);
            Console.WriteLine(model.GeometryName);
            model.PartRole = WiCAM.Pn4000.BendModel.BendTools.PartRole.BendModel;
            Console.WriteLine(model.PartRole);
            Console.WriteLine(model.Shell);

            var shell = new Shell(model);
            Console.WriteLine(shell);

            model.Shell = shell;
            Console.WriteLine(model.Shell);

            //  model.Shells.Clear();
            model.Shells.Add(shell);
            Console.WriteLine(model.Shell);
            Console.WriteLine(model.Shells.Count);
            foreach (var shellls in model.Shells)
                shellls.CreateCollisionTree();
            shell.AnalyzeMacros(model, analyzeConfig);
            var bbox = shell.GetWorldMatrix(model);
            Console.WriteLine(bbox);
            xScreen3D.ShowDebugView();
            xScreen3D.BringIntoView();

            var vector3D = new Vector3d { X = 0, Y = 0, Z = 0 };
            xScreen3D.ShowClickSphere(vector3D);
            //     IDocSerializer docSerializer = new WiCAM.Pn4000.BendDocc.DocSerializer(null, null) as IDocSerializer;
            //  IDoc3dFactory doc3dFactory = new WiCAM.Pn4000.BendDocc.Services.Doc3dFactory();
            //  IPnBndDocImporter docImporter = new WiCAM.Pn4000.BendDocc.PnBndDocImporter(docSerializer, doc3dFactory);
            //    var idoc = docImporter.ImportC3DO(c3dFile, true);
            xScreen3D.ScreenD3D.AddModel(model, true, new Action<RenderTaskResult>(ScreenRenderResult));
            xScreen3D.ScreenD3D.ProjectionType = ProjectionType.Perspective;
            xScreen3D.ScreenD3D.ZoomExtend();
            xScreen3D.ScreenD3D.Render(skipQueuedFrames: true);

            xScreen3D.ScreenD3D.SetViewDirectionByMatrix4d(Matrix4d.Identity, render: true);
            xScreen3D.ScreenD3D.UpdateModelGeometry(model, render: true);
            xScreen3D.ScreenD3D.UpdateModelAppearance(model, render: true);

           // var rm = xScreen3D.ScreenD3D.GetRenderedModels();
            //foreach (var m in rm)
              //  System.Diagnostics.Debug.WriteLine(m.Shell);

            if (model.Shells == null || model.Shells.Count == 0 || model.GetAllFaces().Count == 0)
            {
                Console.WriteLine(model.GetAllFaces().Count);
                Console.WriteLine("Das Modell enthält keine Geometrie und kann nicht angezeigt werden.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            xScreen3D.Visibility = Visibility.Visible;
         */
           // RefreshScreen();
        }

        private void ScreenRenderResult(RenderTaskResult result)
        {
            result.RenderTask.Execute(Kernel3DScreen.ScreenD3D.Renderer);
            System.Diagnostics.Debug.WriteLine(result.ToString());
            Kernel3DScreen.ScreenD3D.Render(skipQueuedFrames: true);
            var rm = Kernel3DScreen.ScreenD3D.Renderer.RenderData.FullscreenTriangleRenderer;
            // foreach (var m in rm)
            Console.WriteLine(rm);
            rm.Init(Kernel3DScreen.ScreenD3D.Renderer.Device);
                Console.WriteLine(rm.ToString());
        }
        public void Dispose()
        {
            _clickSphere = null;
            Geometry3dViewer.ScreenD3D?.Dispose();
            //Geometry3dViewer.ScreenD3D = null;
            //DebugInfo?.Dispose();
        }
        private void ViewerLoaded(object sender, RoutedEventArgs e)
        {
            double a = 100.0;
            double a2 = 100.0;
            if (!double.IsNaN(base.Width))
            {
                a = base.Width;
            }

            if (!double.IsNaN(base.Height))
            {
                a2 = base.Height;
            }
            D3DImage InteropImage = new D3DImage();
            Geometry3dViewer = new Screen3D(new ScreenD3D11(new WindowInteropHelper(Application.Current.MainWindow).Handle, InteropImage, (int)Math.Ceiling(a), (int)Math.Ceiling(a2)), _factorio.Resolve<IConfigProvider>());
            
         //   Navigate3DView.DataContext = new Navigate3DViewModel(this, _configProvider);
          //  ScreenInfoView.DataContext = new ScreenInfoViewModel();
           Geometry3dViewer.ScreenD3D.AddModel(_clickSphere); Geometry3dViewer.ScreenD3D.Renderer.RenderData.ShadowMode = RenderData.Shadows.None;
        }

        private void KeyboardClick(object sender, System.Windows.Input.KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ImageHostSizeChanged(object sender, SizeChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void RotationBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           Console.WriteLine(e.Source.ToString());
         //   TransparentCircleSectionButton circleSectionButton = e.Source as TransparentCircleSectionButton;
         //   Console.WriteLine(xScreen3D.Navigate3DView.RotationBtn.Name);
        }

        public void ReadFPRtoEditBox()
        {
            int _index = 0;
            StringBuilder sb = new StringBuilder();
            // textBoxFPR.Text = File.ReadAllText("P:\\u\\pn\\pfiles\\00\\FPR998");
            using (var streamReader = File.OpenText((pathToUserPartSource)))
            {
                /*    
                var lines = streamReader.ReadToEnd().Split("\n".ToCharArray());
              foreach (var line in lines)
              {
                sb.AppendLine(line);
            //    Console.WriteLine(File.ReadAllText("P:\\u\\pn\\pfiles\\00\\FPR998"));
              _index++;
             }
                */
                sb.AppendLine(streamReader.ReadToEnd());

                editor.Text = sb.ToString();
                //   textBoxFPR.Text = sb.ToString();
            }
        }

        public int FindLineNumberFPR(string fileName, string trackText, string oldText, string newText)
        {
            int lineNumber = 0;
            //fileName = "P:" + fileName;
            string[] textLine = System.IO.File.ReadAllLines(fileName);
            for (int i = 0; i < textLine.Length; i++)
            {
                if (textLine[i].Contains(trackText)) //start finding matching text after.
                    traced = true;
                if (traced)
                    if (textLine[i].Contains(oldText)) // Match text
                    {
                        textLine[i] = newText; // replace text with new one.
                        traced = false;
                        System.IO.File.WriteAllLines(fileName, textLine);
                        lineNumber = i;
                        break; //go out from loop
                    }
            }
            ReadFPRtoEditBox();
            return lineNumber;
        }

        private void RenderCtrl_ViewerReady()
        {
           PartEditViewModel._PartEditViewModel.ViewReady();
            
        }

        private void Browser_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Console.WriteLine("Browser_SelectedItemChanged" );
            if (mRenderCtrl.Scene != null)
            mRenderCtrl.ClearScene();
            OnOpenModel(e.NewValue.ToString());


        }

        void OnOpenModel(string s)
        {
            string fileName = "";
            if (string.IsNullOrEmpty(s))
            {
                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    DefaultExt = ".stp",
                    Filter = "Models (*.igs;*.iges;*.stp;*.step;*.brep;*.stl)|*.igs;*.iges;*.stp;*.step;*.brep;*.stl"
                };
                fileName = dlg.FileName;
                if (dlg.ShowDialog() != true)
                    return;
            }
            else
            {
                fileName = s;
            }
            var viewContext = mRenderCtrl.ViewContext;
            viewContext.SetFreeOrbit(true);
            PartEditViewModel.Instance.UserStepFileTree.Clear();
            var featureTreeItem = new UserStepFileTree();
            featureTreeItem.DirectoryPath = fileName;
            PartEditViewModel.Instance.UserStepFileTree.Add(featureTreeItem);
            treeView.ItemsSource = PartEditViewModel.Instance.UserStepFileTree;
            var children = new ObservableCollection<UserStepFile>();

            if (fileName.Contains("dxf") || fileName.Contains(".DXF"))
            {
                var shape = ShapeIO.Open(fileName);
                node = mRenderCtrl.ShowShape(shape, ColorTable.Green);
                mRenderCtrl.ZoomAll();
                featureContext = new FeatureContext(shape);
                return;
            }
            DocumentSceneNode documentSceneNode = new DocumentSceneNode();

            CADReader cADReader = new CADReader();
            cADReader.Open(fileName, (XdeNode xn, AnyCAD.Foundation.TopoShape shape, GTrsf trf, Vector3 color) =>
            {
                // 
                //  mRenderCtrl.ShowSceneNode()
                featureContext = new FeatureContext(shape);
                node = mRenderCtrl.ShowShape(featureContext.GetSolid(), color);
                // node = mRenderCtrl.ShowShape(shape, color);
                //  mRenderCtrl.SetEditor(new MyDistanceMeasureEditor());
                //   var dem = trf.();
                Document document = mRenderCtrl.ViewContext.GetDocument();
                IDoc3d doc3D = _factorio.Resolve<IDoc3dFactory>().CreateDoc("doggi");
                double a1 = 100.0;
                double a2 = 100.0;
                if (!double.IsNaN(this.Width))
                    a1 = this.Width;
                if (!double.IsNaN(this.Height))
                    a2 = this.Height;
           //     Geometry3dViewer.ScreenD3D = new ScreenD3D11(new WindowInteropHelper(Application.Current.MainWindow).Handle, Geometry3dViewer.InteropImage, (int)Math.Ceiling(a1), (int)Math.Ceiling(a2), Geometry3dViewer);
           //   //  IScreenD3D11 screenD3D = _factorio.Resolve<IScreenD3D11>();
           //      Geometry3dViewer.ScreenD3D.AddModel(featureContext.GetSolid());
                Geometry3dViewer.Visibility = Visibility.Visible;

                documentSceneNode.SetDocument(document, mRenderCtrl.Viewer.GetViewId());
                documentSceneNode.SetViewer(mRenderCtrl.Viewer);

                var edgeMaterial = BasicMaterial.Create("hole-edge");
                edgeMaterial.SetColor(ColorTable.Hex(0xFF0000));
                edgeMaterial.SetLineWidth(3);
                // 1. Find the exterial holes
                var holeExp = new HoleDetector();
                if (!holeExp.Initialize(shape))
                    return;
                var holeNumber = holeExp.GetHoleCount();

                for (uint ii = 0; ii < holeNumber; ++ii)
                {
                    var wire = holeExp.GetHoleExteriorWire(ii);
                    wireNode = BrepSceneNode.Create(wire, null, edgeMaterial);
                    Console.WriteLine(wire.GetClassId().ToString());
                    mRenderCtrl.ViewContext.GetScene().AddNode(wireNode);

                    var subTreeItem = new UserStepFile();
                    subTreeItem.Category = wireNode;
                    subTreeItem.Ident = wireNode.GetUuid();
                    subTreeItem.FilePath = wire.GetClassId().ToString();
                    //     subTreeItem.Tag = item.GetName();
                    Console.WriteLine(subTreeItem.Ident);

                    children.Add(subTreeItem);
                    // PickedItem pickedItem = itr.Current().GetUuid();
                    //       }
                }
                var surfaceExp = new AnyCAD.Foundation.SurfaceAnalysisTool(shape);

                var vertexExp = new AnyCAD.Foundation.WireExplor(shape);
                var wwire = vertexExp.GetOuterWire();
                featureTreeItem.Files = children;

                // 2. Show the faces
                var material = MeshStandardMaterial.Create("hole-face");
                material.SetColor(ColorTable.Hex(0xBBAA33));
                material.SetRoughness(0.8f);
                material.SetFaceSide(EnumFaceSide.DoubleSide);

                //  var shapeNode = BrepSceneNode.Create(shape, material, null);
                //  shapeNode.SetDisplayFilter(EnumShapeFilter.Face);
                //  mRenderCtrl.ViewContext.GetScene().AddNode(shapeNode);


                ColorLookupTable clt = new ColorLookupTable();
                clt.SetColorMap(ColorMapKeyword.Create(EnumSystemColorMap.Rainbow));

                float scale = 100;
                clt.SetMinValue(-0.2f * scale);
                clt.SetMaxValue(scale);

                for (uint ii = 0; ii < node.GetFaceCount(); ++ii)
                {
                    var sc = new SurfaceCurvature(node.GetShape());
                    if (sc.Compute(ii, EnumCurvatureType.MeanCurvature))
                    {
                        Console.WriteLine("{0}, {1}", sc.GetMinValue(), sc.GetMaxValue());
                        var colorBuffer = clt.ComputeColors(sc.GetValues(), scale);
                        sc.SetVertexColors(ii, colorBuffer);
                    }
                }

                //  mRenderCtrl.ViewContext.GetScene().AddNode(bs);

                mRenderCtrl.ZoomAll();

                mRenderCtrl.SetSelectCallback((PickedResult callback) =>
                {
                    //       Console.WriteLine(callback.GetItem().GetTopoShapeId());
                    //  Console.WriteLine(callback.GetItem().GetNode());
                    if (BrepSceneNode.Cast(callback.GetItem().GetNode()) != null)
                    {
                        var node = BrepSceneNode.Cast(callback.GetItem().GetNode());
                    }
                    AnyCAD.Foundation.TopoShape topoShape = node.GetTopoShape();
                    node.SetLineWidth(3);
                    node.SetFaceColor(node.GetUuid(), GetColor("Hole"));
                    Console.WriteLine(node.GetUuid());
                });


                //    settings.SetCoordinateWidgetPosition(1);
                //   mRenderCtrl.SetViewCube(EnumViewCoordinateType.AxisAndCube);
                //  mRenderCtrl.Viewer.SetRulerWidget(EnumRulerWidgetType.Empty);
            });
            FeatureEngine engine = new FeatureEngine();
            // if (!engine.Recognize(featureContext, new StringList { "Tube", "Section" }))
            //   return;
            Console.WriteLine(engine.Recognize(featureContext, new StringList { "Square", "Inner" }));


            //fItems.ItemsSource = shapeTreeNode;


            // foreach (var d3child in xScreen3D.MainGrid.Children)
            //  Console.WriteLine(xScreen3D.MainGrid.Children.Add(mRenderCtrl));
            //  CollectFeatures(featureContext, node, shapeTreeNode);
        }

        Vector3 GetColor(string name)
        {
            Vector3 v = ColorTable.Red;
            mFeatureColorTable.TryGetValue(name, out v);
            return v;
        }

        /// <summary>
        /// 收集特征
        /// </summary>
        /// <param name="featureContext"></param>
        /// <param name="node"></param>
        /// <param name="treeItem"></param>
        void CollectFeatures(FeatureContext featureContext, BrepSceneNode node, TreeViewItem treeItem)
        {
            var treeGroup = new ObservableCollection<TreeViewItem>();
            Console.WriteLine(treeItem);
            // 遍历所有组
            foreach (var group in featureContext.GetGroups())
            {
                var featureTreeItem = new TreeViewItem();
                featureTreeItem.Header = group.GetName();
                featureTreeItem.Tag = group.GetName();
                Console.WriteLine(featureTreeItem.Header);
                // 遍历所有特征
                var children = new ObservableCollection<TreeViewItem>();
                foreach (var item in group.GetItems())
                {
                    var subTreeItem = new TreeViewItem();
                    subTreeItem.Header = item.GetName();
                    subTreeItem.Tag = item.GetName();
                    Console.WriteLine(subTreeItem.Header);

                    children.Add(subTreeItem);

                    // 遍历特征上的面信息
                    foreach (var faceGroupName in item.GetFaceGroupNames())
                    {
                        var faceIds = item.GetFaces(faceGroupName);
                        foreach (var faceId in faceIds)
                        {
                            node.SetFaceColor(faceId, GetColor(item.GetName()));
                            Console.WriteLine(GetColor(item.GetName()));
                        }
                    }
                }
                featureTreeItem.ItemsSource = children;

                treeGroup.Add(featureTreeItem);
            }
            treeItem.ItemsSource = treeGroup;
        }
        public class MyDistanceMeasureEditor : DistanceMeasureEditor
        {
            ViewContext ctxx;
            public MyDistanceMeasureEditor()
            {
           
            }
            public override bool Apply(ViewContext ctx)
            {
                ctxx = ctx;
                var node = GetNode();
              //  ctx.SetPickKeyModifier(EnumKeyModifier.KMOD_LCTRL);
             
                //  ctxx.SetPickFilter(EnumShapeFilter.Edge);
                // 获取测量结果

                return base.Apply(ctx);
            }
        }

        public void SavePartEdit()
        {
            Console.WriteLine("saveEditor");
            Console.WriteLine(editor.Text);
            File.WriteAllText(pathToUserPartSource, editor.Text);
            ReadFPRtoEditBox();
        }



        public void CheckToolBar()
        {
            Console.WriteLine("CheckToolBar");
            int iLage = CheckLineNumberToolBar(pathToUserPartSource, " 1  0  0   0        .0000       20     0     0 AusrichtenLage");
            if (iLage == 0)
                MainWindow.mainWindow.xLage.IsChecked = true;
            else MainWindow.mainWindow.xLage.IsChecked = false;
            Console.WriteLine("Lage =   " + MainWindow.mainWindow.xLage.IsChecked);


        }

        private int CheckLineNumberToolBar(string fileName, string trackText)
        {
            int lineNumber = 0;

            Console.WriteLine(fileName);

            string[] textLine = System.IO.File.ReadAllLines(fileName);
            for (int i = 0; i < textLine.Length; i++)
            {
                if (textLine[i].Contains(trackText)) //start finding matching text after.
                    traced = true;
                if (traced)
                    if (textLine[i].Contains(trackText)) // Match text
                    {
                        //textLine[i] = newText; // replace text with new one.
                        traced = false;
                        //System.IO.File.WriteAllLines(fileName, textLine);
                        lineNumber = 1;

                        break; //go out from loop
                    }
            }
            return lineNumber;
        }
        public float[] Color { get; set; } = new float[4];

        private List<Layer> Layer { get; set; } = new();
        public NavigateInteractionMode InteractionMode { get; private set; }
        public Image ImageHost { get; private set; }
        public KeyEventHandler ExternalKeyDown { get; private set; }

        public Vector3 ToColor()
        {
            return new Vector3(Color[0], Color[1], Color[2]);
        }


        [SpecialName]
        object IView.DataContext()
        {
            return this.DataContext;
        }

        [SpecialName]
        void IView.DataContext(object value)
        {
            Console.WriteLine(value);
            this.DataContext = value;
        }

        private void TabItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            PartEditViewModel._PartEditViewModel.RootDirectoryItems.Clear();
            var dlg = new FolderBrowserDialog()
            {
                RootFolder = Environment.SpecialFolder.MyComputer
            };
            dlg.ShowDialog();

            foreach (var file in System.IO.Directory.GetFiles(dlg.SelectedPath))
            {
                if (file.Contains(".stp") || file.Contains(".dxf") || file.Contains(".STP") || file.Contains(".DXF") || file.Contains(".step") || file.Contains(".STEP"))
                {
                    Console.WriteLine(file);

                    PartEditViewModel._PartEditViewModel.RootDirectoryItems.Add(file);

                }
            }
        }

        private void Navigate3DView_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Browser_SelectedItemChanged" + e.OriginalSource);

        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.Source as UserStepFileTree != null)
            {
                var treeView = e.Source as UserStepFileTree;
                var node = treeView.Items as UserStepFile;
                if (node == null)
                    return;
                Console.WriteLine(node.Name);
                Console.WriteLine(node.FilePath);
                Console.WriteLine(node.Ident + "   Ident");
            }
            if (e.NewValue as UserStepFile != null)
            {
                var treeViewItem = e.NewValue as UserStepFile;
                PickedItem pickedItem = new PickedItem();
                pickedItem.SetNode(treeViewItem.Category);
                PickedItemSet pickedItemSet = new PickedItemSet();
                pickedItemSet.Set(pickedItem);
                var node = BrepSceneNode.Cast(treeViewItem.Category);
                AnyCAD.Foundation.TopoShape topoShape = node.GetTopoShape();
                node.SetLineWidth(6);
                node.SetFaceColor(node.GetUuid(), GetColor("Hole"));
                Console.WriteLine(treeViewItem.Name);
            }
            Console.WriteLine(e.NewValue);
            /*      var treeViewItemm = e.NewValue as UserStepFile;
                  PickedItem pickedItemm = new PickedItem();
                  pickedItemm.SetNode(treeViewItemm.Category);
                  PickedId pickedId = new PickedId(); */
            mRenderCtrl.RequestDraw();
            // pickedItem.SetNode(node.Category.FindChildIndex)
            // pickedItem.SetId()

        }


        private void Image_OpenFolderMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {

        }

  

        private void TabItemRoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("TabRootClick");
            Kernel3DScreen.Visibility = Visibility.Visible;
            mRenderCtrl.Visibility = Visibility.Hidden;
        }

        private void TabItemTest_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Kernel3DScreen.Visibility = Visibility.Visible;
            AnyCad3Dscreen.Visibility= Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Kernel3DScreen.Visibility = Visibility.Hidden;
            AnyCad3Dscreen.Visibility = Visibility.Visible;
        }

        private void KernelScreenPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void GridSplitterPropertyPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void GridSplitterPropertyPanel_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void GridSplitterPropertyPanel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
    internal class Shape
    {
        public string Type { get; set; } = string.Empty;
        public double[] Data { get; set; } = new double[4];
        public string Content { get; set; } = string.Empty;
        public void Show(IRenderView render, MaterialInstance material)
        {
            if (Type == "Polyline")
            {
                if (Data.Length == 4)
                {
                    var p1 = new GPnt2d(Data[0], Data[1]);
                    var p2 = new GPnt2d(Data[2], Data[3]);
                    if (p1.Distance(p2) < 0.01)
                        return;
                    var shape = Sketch2dBuilder.MakeLine(p1, p2);
                    var node = BrepSceneNode.Create(shape, material, material);
                    render.ShowSceneNode(node);
                }
                else if (Data.Length > 4)
                {
                    var pts = new GPntList();
                    for (int ii = 0; ii < Data.Length / 2; ++ii)
                    {
                        pts.Add(new GPnt(Data[ii * 2], Data[ii * 2 + 1], 0));
                    }
                    pts.Add(new GPnt(Data[0], Data[1], 0));
                    var shape = SketchBuilder.MakePolyline(pts);
                    var node = BrepSceneNode.Create(shape, material, material);
                    render.ShowSceneNode(node);
                }

            }
            else if (Type == "Text")
            {
                if (Content.Length > 0)
                {
                    var mesh = AnyCAD.Foundation.FontManager.Instance().CreateMesh(Content);
                    var node = PrimitiveSceneNode.Create(mesh, material);
                    var trf = AnyCAD.Foundation.Matrix4.makeTranslation((float)Data[0], (float)Data[1], 0);
                    float height = 0.002f * (float)Data[4];
                    float width = height / (float)Data[5];
                    var scale = AnyCAD.Foundation.Matrix4.makeScale(height, width, 1);
                    var rotate = AnyCAD.Foundation.Matrix4.makeRotation(Vector3.UNIT_X, new Vector3((float)Data[2], (float)Data[3], 0));
                    node.SetTransform(trf * rotate * scale);
                    render.ShowSceneNode(node);
                }

            }
            else if (Type == "Circle")
            {
                var pt = new GPnt2d(Data[0], Data[1]);
                var shape = Sketch2dBuilder.MakeCircle(new GCirc2d(new GAx2d(pt, new GDir2d(1, 0)), Data[2]));
                var node = BrepSceneNode.Create(shape, material, material);
                render.ShowSceneNode(node);
            }
            else if (Type == "CircArc")
            {
                var pt = new GPnt2d(Data[0], Data[1]);
                var radius = Data[2];
                var start = Data[3];
                var end = Data[4];
                var refVec = new GDir2d(Data[5], Data[6]);
                var shape = Sketch2dBuilder.MakeArc(new GCirc2d(new GAx2d(pt, refVec), radius), start, end);
                var node = BrepSceneNode.Create(shape, material, material);
                render.ShowSceneNode(node);
            }
        }
    }

    internal class Layer
    {
        public uint Id { get; set; } = 0;

        public string Name { get; set; } = string.Empty;

        public float[] Color { get; set; } = new float[4];
    }

    internal class Entity
    {
        public uint Id { get; set; } = 0;
        public uint LayerId { get; set; } = 0;

        public string Type { get; set; } = string.Empty;

        public double[] Extends { get; set; } = new double[4];

        public float[] Color { get; set; } = new float[3];

        public List<Entity> Children { get; set; } = new();

        public List<Shape> Geometry { get; set; } = new();


        public Vector3 ToColor()
        {
            return new Vector3(Color[0], Color[1], Color[2]);
        }

        public void Show(IRenderView render, MaterialInstance material)
        {
            foreach (var shape in Children)
            {
                shape.Show(render, material);
            }

            foreach (var shape in Geometry)
            {
                shape.Show(render, material);
            }
        }
    }


    internal class DrawingDb
    {
        public List<Entity> Entity { get; set; } = new();
        public List<Layer> Layer { get; set; } = new();

         public DrawingDb? Load(string fileName)
        {
            Console.WriteLine("drawingDBLoad");
            using (StreamReader reader = new StreamReader(fileName))
            {
                var data = reader.ReadToEnd();
                return JsonSerializer.Deserialize<DrawingDb>(data);
            }
        }

        public void Show(IRenderView render)
        {
            Console.WriteLine("drawingDB");
            var materials = new Dictionary<uint, MaterialInstance>();
            foreach (var layer in Layer)
            {
                Console.WriteLine("xxxxdLayer");
                var material = BasicMaterial.Create("cad");
                material.SetColor(new Vector3(layer.Color[0], layer.Color[1], layer.Color[2]));

                materials.Add(layer.Id, material);
            }
            var defalutMaterial = BasicMaterial.Create("cad");
            defalutMaterial.SetColor(ColorTable.Purple);

            foreach (var shape in Entity)
            {
                Console.WriteLine("xxxxd");
                materials.TryGetValue(shape.LayerId, out var material);
                if (material != null)
                    shape.Show(render, material);
                else
                    shape.Show(render, defalutMaterial);
            }
        }
    }

}