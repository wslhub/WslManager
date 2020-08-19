using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace WslManager
{
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
    }

    public abstract class CodeFirstForm<T> : CodeFirstForm
        where T : class, INotifyPropertyChanged
    {
        public CodeFirstForm(T viewModel)
            : base()
        {
            _viewModel = viewModel;
        }

        public CodeFirstForm()
            : this(default)
        { }

        protected override void InitializeFields()
        {
            base.InitializeFields();

            if (_viewModel == default)
                _viewModel = CreateDefaultViewModel();

            if (_viewModel == default)
                throw new InvalidOperationException("View model should not be a null reference.");
        }

        private T _viewModel;

        public abstract T CreateDefaultViewModel();

        public T ViewModel
        {
            get => _viewModel;
        }
    }
}
