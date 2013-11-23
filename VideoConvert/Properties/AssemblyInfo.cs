//============================================================================
// VideoConvert - Fast Video & Audio Conversion Tool
// Copyright © 2012 JT-Soft
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//=============================================================================

using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows;

// Allgemeine Informationen über eine Assembly werden über die folgenden 
// Attribute gesteuert. Ändern Sie diese Attributwerte, um die Informationen zu ändern,
// die mit einer Assembly verknüpft sind.
[assembly: AssemblyTitle("VideoConvert")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("JT-Soft")]
[assembly: AssemblyProduct("VideoConvert")]
[assembly: AssemblyCopyright("Copyright © JT-Soft 2011-2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: GuidAttribute("6109EC7F-753E-438A-B5CB-01A2117B3EE6")]

// Durch Festlegen von ComVisible auf "false" werden die Typen in dieser Assembly unsichtbar 
// für COM-Komponenten. Wenn Sie auf einen Typ in dieser Assembly von 
// COM zugreifen müssen, legen Sie das ComVisible-Attribut für diesen Typ auf "true" fest.
[assembly: ComVisible(false)]

//Um mit dem Erstellen lokalisierbarer Anwendungen zu beginnen, legen Sie 
//<UICulture>ImCodeVerwendeteKultur</UICulture> in der .csproj-Datei
//in einer <PropertyGroup> fest. Wenn Sie in den Quelldateien beispielsweise Deutsch
//(Deutschland) verwenden, legen Sie <UICulture> auf \"de-DE\" fest. Heben Sie dann die Auskommentierung
//des nachstehenden NeutralResourceLanguage-Attributs auf. Aktualisieren Sie "en-US" in der nachstehenden Zeile,
//sodass es mit der UICulture-Einstellung in der Projektdatei übereinstimmt.

[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //Speicherort der designspezifischen Ressourcenwörterbücher
    //(wird verwendet, wenn eine Ressource auf der Seite
    // oder in den Anwendungsressourcen-Wörterbüchern nicht gefunden werden kann.)
    ResourceDictionaryLocation.SourceAssembly //Speicherort des generischen Ressourcenwörterbuchs
    //(wird verwendet, wenn eine Ressource auf der Seite, in der Anwendung oder einem 
    // designspezifischen Ressourcenwörterbuch nicht gefunden werden kann.)
)]


// Versionsinformationen für eine Assembly bestehen aus den folgenden vier Werten:
//
//      Hauptversion
//      Nebenversion 
//      Buildnummer
//      Revision
//
// Sie können alle Werte angeben oder die standardmäßigen Build- und Revisionsnummern 
// übernehmen, indem Sie "*" eingeben:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion("0.33.1025.1248")]
[assembly: AssemblyFileVersion("0.33.1025.1248")]
