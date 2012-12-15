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

namespace Xceed.Wpf.Toolkit.Themes
{
  public sealed class StaticResourceKey : ResourceKey
  {
    private string _key;
    public string Key
    {
      get
      {
        return _key;
      }
    }

    private Type _type;
    public Type Type
    {
      get
      {
        return _type;
      }
    }

    public StaticResourceKey( Type type, string key )
    {
      _type = type;
      _key = key;
    }

    public override System.Reflection.Assembly Assembly
    {
      get
      {
        return _type.Assembly;
      }
    }
  }
}
