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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  public class PropertyItemCollection : ObservableCollection<PropertyItem>
  {
    public PropertyItemCollection()
    {

    }

    public PropertyItemCollection( List<PropertyItem> list )
      : base( list )
    {

    }
    public PropertyItemCollection( IEnumerable<PropertyItem> collection )
      : base( collection )
    {

    }

    private ICollectionView GetDefaultView()
    {
      return CollectionViewSource.GetDefaultView( this );
    }

    public void GroupBy( string name )
    {
      GetDefaultView().GroupDescriptions.Add( new PropertyGroupDescription( name ) );
    }

    public void SortBy( string name, ListSortDirection sortDirection )
    {
      GetDefaultView().SortDescriptions.Add( new SortDescription( name, sortDirection ) );
    }

    public void Filter( string text )
    {
      if( text == null )
        return;

      GetDefaultView().Filter = ( item ) =>
      {
        var property = item as PropertyItem;
        return property.DisplayName.ToLower().StartsWith( text.ToLower() );
      };
    }
  }
}
