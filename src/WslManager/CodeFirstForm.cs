using System.ComponentModel;
using System.Windows.Forms;

namespace WslManager
{
    public abstract class CodeFirstForm<T> : CodeFirstForm
        where T : class
    {
        protected override void InitializeFields()
        {
            base.InitializeFields();

            _model = CreateDefaultModel();
        }

        private T _model;

        public abstract T CreateDefaultModel();

        public override object GetModel() => _model;

        public override void SetModel(object model) => _model = (T)model;

        public T Model
        {
            get => _model;
            set => _model = value;
        }
    }

    [DesignerCategory(default)]
    public abstract class CodeFirstForm : Form
    {
        public CodeFirstForm()
            : base()
        {
            Initialize();
        }

        private IContainer _components = default;

        private void Initialize()
        {
            InitializeFields();

            _components = new Container();
            InitializeComponents(_components);

            SuspendLayout();
            InitializeUserInterface();
            ResumeLayout();
        }

        protected virtual void InitializeFields() { }

        protected virtual void InitializeComponents(IContainer components) { }

        protected virtual void InitializeUserInterface() { }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _components?.Dispose();
                _components = null;
            }

            base.Dispose(disposing);
        }

        public virtual object GetModel() { return default; }

        public virtual void SetModel(object model) { }
    }
}
