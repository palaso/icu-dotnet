// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Icu
{
	public enum UTransDirection
	{
		UTRANS_FORWARD,
		UTRANS_REVERSE
	}

	public class Transliterator : SafeHandle
	{
		#region Static Methods
		/// <summary>
		/// Shortcut method equivalent to `CreateInstance(id, UTransDirection.Forward, null)`.
		/// </summary>
		/// <param name="id"></param>
		/// <returns>
		/// </returns>
		public static Transliterator CreateInstance(string id)
		{
			return CreateInstance(id, UTransDirection.UTRANS_FORWARD, null);
		}

		/// <summary>
		/// Get an ICU Transliterator.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="dir"></param>
		/// <param name="rules"></param>
		/// <returns>
		/// A Transliterator class instance. Be sure to call the instance's `Dispose` method to clean up.
		/// </returns>
		public static Transliterator CreateInstance(string id, UTransDirection dir, string rules)
		{
			Transliterator result = NativeMethods.utrans_openU(id, dir, rules, out ParseError parseError, out ErrorCode status);
			ExceptionFromErrorCode.ThrowIfError(status);
			
			return result;
		}

		/// <summary>
		/// Get an ICU UEnumeration pointer that will enumerate all transliterator IDs.
		/// </summary>
		/// <returns>The opaque UEnumeration pointer. Closing it properly is the responsibility of the caller.</returns>
		private static SafeEnumeratorHandle GetEnumerator()
		{
			var result = NativeMethods.utrans_openIDs(out var status);
			ExceptionFromErrorCode.ThrowIfError(status);
			return result;
		}

		/// <summary>
		/// Get the IDs available at the time of the call, including user-registered IDs.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<string> GetAvailableIds()
		{
			using (var icuEnumerator = GetEnumerator())
			{
				for (var id = icuEnumerator.Next(); !string.IsNullOrEmpty(id); id = icuEnumerator.Next())
				{
					yield return id;
				}
			}
		}

		/// <summary>
		/// Get the IDs and display names of all transliterators registered with ICU.
		/// Display names will be in the locale specified by the displayLocale parameter; omit it or pass in null to use the default locale.
		/// </summary>
		public static IEnumerable<(string id, string name)> GetIdsAndNames(string displayLocale = null)
		{
			using (var icuEnumerator = GetEnumerator())
			{
				for (var id = icuEnumerator.Next(); !string.IsNullOrEmpty(id); id = icuEnumerator.Next())
				{
					var name = GetDisplayName(id, displayLocale);
					if (name != null)
						yield return (id, name);
				}
			}
		}

		/// <summary>
		/// Reimplementation of TransliteratorIDParser::IDtoSTV from the ICU C++ API. Parses a
		/// transliterator ID in one of several formats and returns the source, target and variant
		/// components of the ID. Valid formats are T, T/V, S-T, S-T/V, or S/V-T. If source is
		/// missing, it will be set to "Any". If target or variant is missing, they will be the
		/// empty string. (If target is missing, the ID is not well-formed, but this function will
		/// not throw an exception). If variant is present, the slash will be included in.
		/// </summary>
		/// <param name="transId">Transliterator ID to parse</param>
		/// <param name="source">"Any" if source was missing, otherwise source component of the
		/// ID</param>
		/// <param name="target">Empty string if no target, otherwise target component of the ID
		/// (should always be present in a well-formed ID)</param>
		/// <param name="variant">Empty string if no variant, otherwise variant component of the
		/// ID, *with a '/' as its first character*.</param>
		/// <returns>True if source was present, false if source was missing.</returns>
		private static bool ParseTransliteratorID(string transId, out string source,
			out string target, out string variant)
		{
			// This is a straight port of the TransliteratorIDParser::IDtoSTV logic, with basically no changes
			source = "Any";
			var tgtSep = transId.IndexOf("-");
			var varSep = transId.IndexOf("/");
			if (varSep < 0)
				varSep = transId.Length;

			var isSourcePresent = false;
			if (tgtSep < 0)
			{
				// Form: T/V or T (or /V)
				target = transId.Substring(0, varSep);
				variant = transId.Substring(varSep);
			}
			else if (tgtSep < varSep)
			{
				// Form: S-T/V or S-T (or -T/V or -T)
				if (tgtSep > 0)
				{
					source = transId.Substring(0, tgtSep);
					isSourcePresent = true;
				}
				target = transId.Substring(tgtSep + 1, varSep - tgtSep - 1);
				variant = transId.Substring(varSep);
			}
			else
			{
				// Form: S/V-T or /V-T
				if (varSep > 0)
				{
					source = transId.Substring(0, varSep);
					isSourcePresent = true;
				}
				variant = transId.Substring(varSep, tgtSep - varSep);
				target = transId.Substring(tgtSep + 1);
			}

			// The above Substring calls have all left the variant either empty or looking like "/V". In the original C++
			// implementation, we removed the leading "/". But here, we keep it because the only code that needs to call
			// this is GetTransliteratorDisplayName, which wants the leading "/" on variant names.
			//if (variant.Length > 0)
			//	variant = variant.Substring(1);

			return isSourcePresent; // This is currently not used, but we return it anyway for compatibility with original C++ implementation
		}

		/// <summary>
		/// Get a display name for a transliterator. This reimplements the logic from the C++
		/// Transliterator::getDisplayName method, since the ICU C API doesn't expose a
		/// utrans_getDisplayName() call. (Unfortunately).
		/// Note that if no text is found for the given locale, ICU will (by default) fallback to
		/// the root locale. However, the root locale's strings for transliterator display names
		/// are ugly and not suitable for displaying to the user. Therefore, if we have to
		/// fallback, we fallback to the "en" locale instead of the root locale.
		/// </summary>
		/// <param name="transId">The translator's system ID in ICU.</param>
		/// <param name="localeName">The ICU name of the locale in which to calculate the display
		/// name.</param>
		/// <returns>A name suitable for displaying to the user in the given locale, or in English
		/// if no translated text is present in the given locale.</returns>
		public static string GetDisplayName(string transId, string localeName)
		{
			const string translitDisplayNameRBKeyPrefix = "%Translit%%";  // See RB_DISPLAY_NAME_PREFIX in translit.cpp in ICU source code
			const string scriptDisplayNameRBKeyPrefix = "%Translit%";  // See RB_SCRIPT_DISPLAY_NAME_PREFIX in translit.cpp in ICU source code
			const string translitResourceBundleName = "ICUDATA-translit";
			const string translitDisplayNamePatternKey = "TransliteratorNamePattern";

			ParseTransliteratorID(transId, out var source, out var target, out var variant);
			if (target.Length < 1)
				return transId;  // Malformed ID? Give up

			using (var bundle = new ResourceBundle(translitResourceBundleName, localeName))
			using (var bundleFallback = new ResourceBundle(translitResourceBundleName, "en"))
			{
				var pattern = bundle.GetStringByKey(translitDisplayNamePatternKey);
				// If we don't find a MessageFormat pattern in our locale, try the English fallback
				if (string.IsNullOrEmpty(pattern))
					pattern = bundleFallback.GetStringByKey(translitDisplayNamePatternKey);
				// Still can't find a pattern? Then we won't be able to format the ID, so just return it
				if (string.IsNullOrEmpty(pattern))
					return transId;

				// First check if there is a specific localized name for this transliterator, and if so, just return it.
				// Note that we need to check whether the string we got still starts with the "%Translit%%" prefix, because
				// if so, it means that we got a value from the root locale's bundle, which isn't actually localized.
				var translitLocalizedName = bundle.GetStringByKey(translitDisplayNameRBKeyPrefix + transId);

				if (!string.IsNullOrEmpty(translitLocalizedName) && !translitLocalizedName.StartsWith(translitDisplayNameRBKeyPrefix))
					return translitLocalizedName;

				// There was no specific localized name for this transliterator (which will be true of most cases). Build one.

				// Try getting localized display names for the source and target, if possible.
				var localizedSource = bundle.GetStringByKey(scriptDisplayNameRBKeyPrefix 
				+ source);
				if (string.IsNullOrEmpty(localizedSource))
				{
					localizedSource = source; // Can't localize
				}
				else
				{
					// As with the transliterator name, we need to check that the string we got didn't come from the root bundle
					// (which just returns a string that still contains the ugly %Translit% prefix). If it did, fall back to English.
					if (localizedSource.StartsWith(scriptDisplayNameRBKeyPrefix))
						localizedSource = bundleFallback.GetStringByKey(scriptDisplayNameRBKeyPrefix + source);
					if (string.IsNullOrEmpty(localizedSource) || localizedSource.StartsWith(scriptDisplayNameRBKeyPrefix))
						localizedSource = source;
				}

				// Same thing for target
				var localizedTarget = bundle.GetStringByKey
				(scriptDisplayNameRBKeyPrefix + target);
				if (string.IsNullOrEmpty(localizedTarget))
				{
					localizedTarget = target; // Can't localize
				}
				else
				{
					if (localizedTarget.StartsWith(scriptDisplayNameRBKeyPrefix))
						localizedTarget = bundleFallback.GetStringByKey(scriptDisplayNameRBKeyPrefix + target);
					if (string.IsNullOrEmpty(localizedTarget) || localizedTarget.StartsWith(scriptDisplayNameRBKeyPrefix))
						localizedTarget = target;
				}

				var displayName = MessageFormatter.Format(pattern, localeName, out var status,
					2.0, localizedSource, localizedTarget);
				if (status.IsSuccess())
					return displayName + variant; // Variant is either empty string or starts with "/"
				return transId; // If formatting fails, the transliterator's ID is still our final fallback
			}
		}
		#endregion

		#region Instance Methods
		#region SafeHandle overrides
		internal Transliterator() : base(IntPtr.Zero, true)
		{ }

		public override bool IsInvalid => handle == IntPtr.Zero;

		protected override bool ReleaseHandle()
		{
			NativeMethods.utrans_close(handle);
			return true;
		}
		#endregion

		/// <summary>
		/// Transliterate `text`.
		/// </summary>
		/// <param name="text"></param>
		/// <returns>
		/// The transliterated text, truncated to `text.Length` characters.
		/// </returns>
		public string Transliterate(string text)
		{
			string result = NativeMethods.utrans_transUChars(handle, text, out ErrorCode status);
			ExceptionFromErrorCode.ThrowIfError(status);
			return result;
		}

		#endregion
	}
}
