﻿using Avalonia.Controls;
using Avalonia.Platform.Interop;

namespace Avalonia.Native.Interop
{
    public partial class IAvnAppMenuItem
    {
        private IAvnAppMenu _subMenu;
        private AvaloniaNativeMenuExporter _exporter;

        public NativeMenuItemBase Managed { get; set; }

        internal void Update(AvaloniaNativeMenuExporter exporter, IAvaloniaNativeFactory factory, NativeMenuItem item)
        {
            _exporter = exporter;

            Managed = item;

            Managed.PropertyChanged += Item_PropertyChanged;

            using (var buffer = new Utf8Buffer(item.Header))
            {
                Title = buffer.DangerousGetHandle();
            }

            if (item.Gesture != null)
            {
                using (var buffer = new Utf8Buffer(OsxUnicodeKeys.ConvertOSXSpecialKeyCodes(item.Gesture.Key)))
                {
                    SetGesture(buffer.DangerousGetHandle(), (AvnInputModifiers)item.Gesture.KeyModifiers);
                }
            }

            SetAction(new PredicateCallback(() =>
            {
                if (item.Command != null || item.HasClickHandlers)
                {
                    return item.Enabled;
                }

                return false;
            }), new MenuActionCallback(() => { item.RaiseClick(); }));

            if (item.Menu != null)
            {
                if (_subMenu == null)
                {
                    _subMenu = factory.CreateMenu();
                }

                _subMenu.Update(exporter, factory, item.Menu);
            }

            if (item.Menu == null && _subMenu != null)
            {
                _subMenu.Cleanup();

                // todo remove submenu.

                // needs implementing on native side also.
            }
        }

        private void Item_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            _exporter.QueueReset();
        }

        internal void Cleanup()
        {
            Managed.PropertyChanged -= Item_PropertyChanged;

            if (_subMenu != null)
            {
                _subMenu.Cleanup();
            }

            _subMenu = null;
            _exporter = null;
            Managed = null;            
        }
    }
}