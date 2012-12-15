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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit
{
  public class ColorSpectrumSlider : Slider
  {
    #region Private Members

    private Rectangle _spectrumDisplay;
    private LinearGradientBrush _pickerBrush;

    #endregion //Private Members

    #region Constructors

    static ColorSpectrumSlider()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( ColorSpectrumSlider ), new FrameworkPropertyMetadata( typeof( ColorSpectrumSlider ) ) );
    }

    #endregion //Constructors

    #region Dependency Properties

    public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register( "SelectedColor", typeof( Color ), typeof( ColorSpectrumSlider ), new PropertyMetadata( System.Windows.Media.Colors.Transparent ) );
    public Color SelectedColor
    {
      get
      {
        return ( Color )GetValue( SelectedColorProperty );
      }
      set
      {
        SetValue( SelectedColorProperty, value );
      }
    }

    #endregion //Dependency Properties

    #region Base Class Overrides

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      _spectrumDisplay = ( Rectangle )GetTemplateChild( "PART_SpectrumDisplay" );
      CreateSpectrum();
      OnValueChanged( Double.NaN, Value );
    }

    protected override void OnValueChanged( double oldValue, double newValue )
    {
      base.OnValueChanged( oldValue, newValue );

      Color color = ColorUtilities.ConvertHsvToRgb( 360 - newValue, 1, 1 );
      SelectedColor = color;
    }

    #endregion //Base Class Overrides

    #region Methods

    private void CreateSpectrum()
    {
      _pickerBrush = new LinearGradientBrush();
      _pickerBrush.StartPoint = new Point( 0.5, 0 );
      _pickerBrush.EndPoint = new Point( 0.5, 1 );
      _pickerBrush.ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;

      List<Color> colorsList = ColorUtilities.GenerateHsvSpectrum();

      double stopIncrement = ( double )1 / colorsList.Count;

      int i;
      for( i = 0; i < colorsList.Count; i++ )
      {
        _pickerBrush.GradientStops.Add( new GradientStop( colorsList[ i ], i * stopIncrement ) );
      }

      _pickerBrush.GradientStops[ i - 1 ].Offset = 1.0;
      _spectrumDisplay.Fill = _pickerBrush;
    }

    #endregion //Methods
  }
}
