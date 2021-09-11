/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using UnityEditor.Compilation;

namespace Microsoft.Unity.VisualStudio.Editor
{
	internal static class UnityInstallation
	{
		public static Version LatestLanguageVersionSupported(Assembly assembly)
		{
#if UNITY_2020_2_OR_NEWER
			if (assembly?.compilerOptions != null && Version.TryParse(assembly.compilerOptions.LanguageVersion, out var result))
				return result;

			// if parsing fails, we know at least we have support for 8.0
			return new Version(8, 0);
#else
			return new Version(7, 3);
#endif
		}

	}
}
