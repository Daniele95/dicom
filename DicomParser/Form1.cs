using CSharpGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using Kitware.VTK;

namespace VolumeRendering.Raycast
{
    public partial class Form1 : Form
    {
        public static Boolean CSHARPGL = false;

        //PARTE DI ACTIVIZ-------------------------------------------------------------------------------------
        // module wide accessible variables
        vtkImageViewer2 _ImageViewer;
        vtkTextMapper _SliceStatusMapper;
        int _Slice;
        int _MinSlice;
        int _MaxSlice;
        String studyUID;

        public Form1(string studyUID, Boolean cSHARPGL)
        {
            CSHARPGL = cSHARPGL;
            InitializeComponent();
            this.studyUID = studyUID;


            // parte di CSHARPGL - scommentare per volumetrica
            Console.WriteLine("load form");
            if (CSHARPGL) {  
                this.Load += FormMain_Load;
                this.winGLCanvas1.OpenGLDraw += winGLCanvas1_OpenGLDraw;
                this.winGLCanvas1.Resize += winGLCanvas1_Resize;

                // picking events
                this.winGLCanvas1.MouseDown += glCanvas1_MouseDown;
                this.winGLCanvas1.MouseMove += glCanvas1_MouseMove;
                this.winGLCanvas1.MouseUp += glCanvas1_MouseUp;
                this.winGLCanvas1.MouseWheel += winGLCanvas1_MouseWheel;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            renderWindowControl1_Load(sender, e);

        }

        private void renderWindowControl1_Load(object sender, EventArgs e)
        {
            try
            {
                ReadDICOMSeries();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK);
            }
        }


        private void ReadDICOMSeries()
        {
            // Path to vtk data must be set as an environment variable
            // VTK_DATA_ROOT = "C:\VTK\vtkdata-5.8.0"
            vtkTesting test = vtkTesting.New();
            string root = test.GetDataRoot();
            // Read all the DICOM files in the specified directory.
            // Caution: folder "DicomTestImages" don't exists by default in the standard vtk data folder
            // sample data are available at http://www.vtk.org/Wiki/images/1/12/VTK_Examples_StandardFormats_Input_DicomTestImages.zip
 
            //string folder = Path.Combine(root, @"Data\DicomTestImages");
            string folder = @"provaDicom/"+studyUID;
            Console.WriteLine(folder);
            vtkDICOMImageReader reader = vtkDICOMImageReader.New();
            reader.SetDirectoryName(folder);
            reader.Update();
            // Visualize
            _ImageViewer = vtkImageViewer2.New();
            _ImageViewer.SetInputConnection(reader.GetOutputPort());
            // get range of slices (min is the first index, max is the last index)
            _ImageViewer.GetSliceRange(ref _MinSlice, ref _MaxSlice);
            Debug.WriteLine("slices range from : " + _MinSlice.ToString() + " to " + _MaxSlice.ToString());

            // slice status message
            vtkTextProperty sliceTextProp = vtkTextProperty.New();
            sliceTextProp.SetFontFamilyToCourier();
            sliceTextProp.SetFontSize(20);
            sliceTextProp.SetVerticalJustificationToBottom();
            sliceTextProp.SetJustificationToLeft();

            _SliceStatusMapper = vtkTextMapper.New();
            _SliceStatusMapper.SetInput("Slice No " + (_Slice + 1).ToString() + "/" + (_MaxSlice + 1).ToString());
            _SliceStatusMapper.SetTextProperty(sliceTextProp);

            vtkActor2D sliceStatusActor = vtkActor2D.New();
            sliceStatusActor.SetMapper(_SliceStatusMapper);
            sliceStatusActor.SetPosition(15, 10);
            // usage hint message
            vtkTextProperty usageTextProp = vtkTextProperty.New();
            usageTextProp.SetFontFamilyToCourier();
            usageTextProp.SetFontSize(14);
            usageTextProp.SetVerticalJustificationToTop();
            usageTextProp.SetJustificationToLeft();

            vtkTextMapper usageTextMapper = vtkTextMapper.New();
            usageTextMapper.SetInput("Slice with mouse wheel\nor Up/Down-Key");
            usageTextMapper.SetTextProperty(usageTextProp);

            vtkActor2D usageTextActor = vtkActor2D.New();
            usageTextActor.SetMapper(usageTextMapper);
            usageTextActor.GetPositionCoordinate().SetCoordinateSystemToNormalizedDisplay();
            usageTextActor.GetPositionCoordinate().SetValue(0.05, 0.95);

            vtkRenderWindow renderWindow = renderWindowControl1.RenderWindow;

            vtkInteractorStyleImage interactorStyle = vtkInteractorStyleImage.New();
            // NOTA:non funziona la rotellina del mouse per cambiare slice <--------------------------------------
            // l'errore è causato dalla funzione DicomCFindRequest(della sorgente di FellowOak)
            //in QueryFellowOak.cs, in particolare dal costruttore
            // DicomCFindRequest(DicomQueryRetrieveLevel level)

            // interactorStyle.MouseWheelForwardEvt += new vtkObject.vtkObjectEventHandler(interactor_MouseWheelForwardEvt);
            //  interactorStyle.MouseWheelBackwardEvt += new vtkObject.vtkObjectEventHandler(interactor_MouseWheelBackwardEvt);

            renderWindow.GetInteractor().SetInteractorStyle(interactorStyle);
            renderWindow.GetRenderers().InitTraversal();
            vtkRenderer ren;
            while ((ren = renderWindow.GetRenderers().GetNextItem()) != null)
                ren.SetBackground(0.0, 0.0, 0.0);

            _ImageViewer.SetRenderWindow(renderWindow);
            _ImageViewer.GetRenderer().AddActor2D(sliceStatusActor);
            _ImageViewer.GetRenderer().AddActor2D(usageTextActor);
            _ImageViewer.SetSlice(_MinSlice);
            _ImageViewer.Render();
        }


        /// <summary>
        /// move forward to next slice
        /// </summary>
        private void MoveForwardSlice()
        {
            Debug.WriteLine(_Slice.ToString());
            if (_Slice < _MaxSlice)
            {
                _Slice += 1;
                _ImageViewer.SetSlice(_Slice);
                _SliceStatusMapper.SetInput("Slice No " + (_Slice + 1).ToString() + "/" + (_MaxSlice + 1).ToString());
                _ImageViewer.Render();
            }
        }


        /// <summary>
        /// move backward to next slice
        /// </summary>
        private void MoveBackwardSlice()
        {
            Debug.WriteLine(_Slice.ToString());
            if (_Slice > _MinSlice)
            {
                _Slice -= 1;
                _ImageViewer.SetSlice(_Slice);
                _SliceStatusMapper.SetInput("Slice No " + (_Slice + 1).ToString() + "/" + (_MaxSlice + 1).ToString());
                _ImageViewer.Render();
            }
        }


        /// <summary>
        /// eventhanndler to process keyboard input
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            //Debug.WriteLine(DateTime.Now + ":" + msg.Msg + ", " + keyData);
            if (keyData == System.Windows.Forms.Keys.Up)
            {
                MoveForwardSlice();
                return true;
            }
            else if (keyData == System.Windows.Forms.Keys.Down)
            {
                MoveBackwardSlice();
                return true;
            }
            // don't forward the following keys
            // add all keys which are not supposed to get forwarded
            else if (
                  keyData == System.Windows.Forms.Keys.F
               || keyData == System.Windows.Forms.Keys.L
            )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// event handler for mousewheel forward event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void interactor_MouseWheelForwardEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            MoveForwardSlice();
        }


        /// <summary>
        /// event handler for mousewheel backward event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void interactor_MouseWheelBackwardEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            MoveBackwardSlice();
        }


        // parte di CSharpGL--------------------------------------------------------------------------------------------

        private Scene scene;
        private ActionList actionList;

        private Picking pickingAction;
        private LegacyTriangleNode triangleTip;
        private LegacyQuadNode quadTip;

        void winGLCanvas1_MouseWheel(object sender, MouseEventArgs e)
        {
            var scene = this.scene;
            if (scene != null)
            {
                scene.Camera.MouseWheel(e.Delta);
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            var position = new vec3(5, 3, 4) * 0.2f;
            var center = new vec3(0, 0, 0);
            var up = new vec3(0, 1, 0);
            var camera = new Camera(position, center, up, CameraType.Perspecitive, this.winGLCanvas1.Width, this.winGLCanvas1.Height);

            this.scene = new Scene(camera)
;
            {
                var manipulater = new ArcBallManipulater(GLMouseButtons.Left | GLMouseButtons.Right);
                manipulater.Bind(camera, this.winGLCanvas1);
                manipulater.Rotated += manipulater_Rotated;
                Console.WriteLine("pick");
                var node = RaycastNode.Create();
                this.scene.RootNode = node;
                (new FormProperyGrid(node)).Show();
            }

            var list = new ActionList();
            var transformAction = new TransformAction(scene.RootNode);
            list.Add(transformAction);
            var renderAction = new RenderAction(scene);
            list.Add(renderAction);
            this.actionList = list;

            this.pickingAction = new Picking(scene);

            this.triangleTip = new LegacyTriangleNode();
            this.quadTip = new LegacyQuadNode();

        }

        void manipulater_Rotated(object sender, ArcBallManipulater.Rotation e)
        {
            SceneNodeBase node = this.scene.RootNode;
            node.RotationAngle = e.angleInDegree;
            node.RotationAxis = e.axis;
        }

        private void winGLCanvas1_OpenGLDraw(object sender, PaintEventArgs e)
        {
            ActionList list = this.actionList;
            if (list != null)
            {
                vec4 clearColor = this.scene.ClearColor;
                GL.Instance.ClearColor(clearColor.x, clearColor.y, clearColor.z, clearColor.w);
                GL.Instance.Clear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT | GL.GL_STENCIL_BUFFER_BIT);

                list.Act(new ActionParams(Viewport.GetCurrent()));
            }
        }

        void winGLCanvas1_Resize(object sender, EventArgs e)
        {
            this.scene.Camera.AspectRatio = ((float)this.winGLCanvas1.Width) / ((float)this.winGLCanvas1.Height);
        }






    }
}