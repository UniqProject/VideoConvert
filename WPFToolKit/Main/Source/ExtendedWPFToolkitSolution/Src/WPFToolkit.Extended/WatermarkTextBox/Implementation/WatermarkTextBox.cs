﻿/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Reciprocal
   License (Ms-RL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xceed.Wpf.Toolkit
{
  public class WatermarkTextBox : TextBox
  {
    #region Properties

    #region SelectAllOnGotFocus

    public static readonly DependencyProperty SelectAllOnGotFocusProperty = DependencyProperty.Register( "SelectAllOnGotFocus", typeof( bool ), typeof( WatermarkTextBox ), new PropertyMetadata( false ) );
    public bool SelectAllOnGotFocus
    {
      get
      {
        return ( bool )GetValue( SelectAllOnGotFocusProperty );
      }
      set
      {
        SetValue( SelectAllOnGotFocusProperty, value );
      }
    }

    #endregion //SelectAllOnGotFocus

    #region Watermark

    public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register( "Watermark", typeof( object ), typeof( WatermarkTextBox ), new UIPropertyMetadata( null ) );
    public object Watermark
    {
      get
      {
        return ( object )GetValue( WatermarkProperty );
      }
      set
      {
        SetValue( WatermarkProperty, value );
      }
    }

    #endregion //Watermark

    #region WatermarkTemplate

    public static readonly DependencyProperty WatermarkTemplateProperty = DependencyProperty.Register( "WatermarkTemplate", typeof( DataTemplate ), typeof( WatermarkTextBox ), new UIPropertyMetadata( null ) );
    public DataTemplate WatermarkTemplate
    {
      get
      {
        return ( DataTemplate )GetValue( WatermarkTemplateProperty );
      }
      set
      {
        SetValue( WatermarkTemplateProperty, value );
      }
    }

    #endregion //WatermarkTemplate

    #endregion //Properties

    #region Constructors

    static WatermarkTextBox()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( WatermarkTextBox ), new FrameworkPropertyMetadata( typeof( WatermarkTextBox ) ) );
    }

    #endregion //Constructors

    #region Base Class Overrides

    protected override void OnGotKeyboardFocus( KeyboardFocusChangedEventArgs e )
    {
      base.OnGotKeyboardFocus( e );

      if( SelectAllOnGotFocus )
        SelectAll();
    }

    protected override void OnPreviewMouseLeftButtonDown( MouseButtonEventArgs e )
    {
      if( !IsKeyboardFocused && SelectAllOnGotFocus )
      {
        e.Handled = true;
        Focus();
      }

      base.OnPreviewMouseLeftButtonDown( e );
    }

    #endregion //Base Class Overrides
  }
}
