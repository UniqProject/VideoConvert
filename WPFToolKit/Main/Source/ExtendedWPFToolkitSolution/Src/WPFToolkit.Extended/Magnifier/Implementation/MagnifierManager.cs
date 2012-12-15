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

using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace Xceed.Wpf.Toolkit
{
  public class MagnifierManager : DependencyObject
  {
    #region Members

    private MagnifierAdorner _adorner;
    private UIElement _element;

    #endregion //Members

    #region Properties

    public static readonly DependencyProperty CurrentProperty = DependencyProperty.RegisterAttached( "Magnifier", typeof( Magnifier ), typeof( UIElement ), new FrameworkPropertyMetadata( null, OnMagnifierChanged ) );
    public static void SetMagnifier( UIElement element, Magnifier value )
    {
      element.SetValue( CurrentProperty, value );
    }
    public static Magnifier GetMagnifier( UIElement element )
    {
      return ( Magnifier )element.GetValue( CurrentProperty );
    }

    private static void OnMagnifierChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      UIElement target = d as UIElement;

      if( target == null )
        throw new ArgumentException( "Magnifier can only be attached to a UIElement." );

      MagnifierManager manager = new MagnifierManager();
      manager.AttachToMagnifier( target, e.NewValue as Magnifier );
    }

    #endregion //Properties

    #region Event Handlers

    void Element_MouseLeave( object sender, MouseEventArgs e )
    {
      HideAdorner();
    }

    void Element_MouseEnter( object sender, MouseEventArgs e )
    {
      ShowAdorner();
    }

    #endregion //Event Handlers

    #region Methods

    private void AttachToMagnifier( UIElement element, Magnifier magnifier )
    {
      _element = element;
      _element.MouseEnter += Element_MouseEnter;
      _element.MouseLeave += Element_MouseLeave;

      magnifier.Target = _element;

      _adorner = new MagnifierAdorner( _element, magnifier );
    }

    void ShowAdorner()
    {
      VerifyAdornerLayer();
      _adorner.Visibility = Visibility.Visible;
    }

    bool VerifyAdornerLayer()
    {
      if( _adorner.Parent != null )
        return true;

      AdornerLayer layer = AdornerLayer.GetAdornerLayer( _element );
      if( layer == null )
        return false;

      layer.Add( _adorner );
      return true;
    }

    void HideAdorner()
    {
      if( _adorner.Visibility == Visibility.Visible )
      {
        _adorner.Visibility = Visibility.Collapsed;
      }
    }

    #endregion //Methods
  }
}
