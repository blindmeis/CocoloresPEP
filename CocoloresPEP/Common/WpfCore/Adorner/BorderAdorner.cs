using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CocoloresPEP.Common.WpfCore.Adorner
{
    class BorderAdorner : System.Windows.Documents.Adorner
    {
        private readonly Border _child;
        readonly VisualCollection _visualChildren;

        public BorderAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            _child = new Border() { BorderBrush = Brushes.Black, BorderThickness = new Thickness(2) };
            _visualChildren = new VisualCollection(this);
            _visualChildren.Add(_child);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (_child == null)
                return base.MeasureOverride(constraint);

            _child.Width = AdornedElement.RenderSize.Width;
            _child.Height = AdornedElement.RenderSize.Height;

            _child.Measure(AdornedElement.RenderSize);
            return _child.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_child == null)
                return base.ArrangeOverride(finalSize);

            _child.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override int VisualChildrenCount { get { return _visualChildren.Count; } }
        protected override Visual GetVisualChild(int index) { return _visualChildren[index]; }

    }
}
