﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MW5.Tools.Controls.Parameters;
using MW5.Tools.Model;
using MW5.Tools.Model.Parameters;

namespace MW5.Tools.Services
{
    /// <summary>
    /// Routes UI events for the tool to the interested controls.
    /// </summary>
    public class EventManager
    {
        private readonly List<ParameterControlBase> _controls = new List<ParameterControlBase>();
        private Dictionary<string, ParameterControlBase> _dict;
        public event EventHandler<ParameterControlEventArgs> ControlValueChanged;

        public void AddControl(ParameterControlBase control)
        {
            _controls.Add(control);
            control.ValueChanged += (s, e) => FireControlValueChanged(s as ParameterControlBase);
        }

        private void FireControlValueChanged(ParameterControlBase control)
        {
            var handler = ControlValueChanged;
            if (handler != null)
            {
                ControlValueChanged(control, new ParameterControlEventArgs(control));
            }
        }

        public void Bind(ToolConfiguration config)
        {
            _dict = _controls.ToDictionary(p => p.ParameterName);

            BindOutput();

            BindFields(config);

            AssignLayers(config);
        }

        /// <summary>
        /// Binds output filename to the input name.
        /// </summary>
        private void BindOutput()
        {
            var output = _controls.OfType<OutputParameterControl>().FirstOrDefault();
            if (output == null)
            {
                return;
            }

            var input = _controls.OfType<LayerParameterControl>().FirstOrDefault();
            if (input != null)
            {
                input.SelectedLayerChanged += (s, e) => output.OnLayerChanged(e.Datasource);
            }

            var fp = _controls.OfType<FilenameParameterControl>().FirstOrDefault();
            if (fp != null)
            {
                fp.ValueChanged += (s, e) =>
                {
                    var ctrl = s as FilenameParameterControl;
                    if (ctrl != null)
                    {
                        output.OnFilenameChanged(ctrl.GetValue() as string);
                    }
                };
            }
        }

        private ParameterControlBase GetControl(string key)
        {
            return _dict[key];
        }

        private void AssignLayers(ToolConfiguration config)
        {
            foreach (var control in _controls.OfType<LayerParameterControl>())
            {
                control.SetLayers(config.Layers);
            }
        }

        private void BindFields(ToolConfiguration config)
        {
            foreach (var f in config.Fields)
            {
                var layer = GetControl(f.LayerName) as LayerParameterControl;
                var field = GetControl(f.FieldName) as FieldParameterControl;
                
                if (layer != null && field != null)
                {
                    layer.SelectedLayerChanged += (s, e) => field.OnLayerChanged(e.Datasource);
                }
            }
        }
    }
}