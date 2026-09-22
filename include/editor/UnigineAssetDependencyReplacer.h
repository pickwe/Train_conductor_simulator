/* Copyright (C) 2005-2026, UNIGINE. All rights reserved.
*
* This file is a part of the UNIGINE 2 SDK.
*
* Your use and / or redistribution of this software in source and / or
* binary form, with or without modification, is subject to: (i) your
* ongoing acceptance of and compliance with the terms and conditions of
* the UNIGINE License Agreement; and (ii) your inclusion of this notice
* in any version of this software that you use or redistribute.
* A copy of the UNIGINE License Agreement is available by contacting
* UNIGINE. at http://unigine.com/
*/
// DO NOT EDIT DIRECTLY. This is an auto-generated file. Your changes will be lost.

#pragma once

#include <UnigineCallback.h>
#include <UnigineGUID.h>
#include <UniginePtr.h>
#include <UnigineString.h>
#include <UnigineVector.h>

#include "UnigineEditorGlobal.h"
#include "UnigineCollection.h"

namespace UnigineEditor
{


/// <summary> A mutation record passed to a dependency replacer while it rewrites the references of a single file: it carries the file's GUID and the asset-import and per-runtime custom parameters used for that file. A replacer reads the parameters and may update them through the setters; if it does, the dependency replacement system applies the parameter update for all affected assets on the main thread.
/// The parameter getters (<see cref='getAssetImportParameters'/> / <see cref='getCustomParameters'/>) always return a live <see cref='Collection'/>, never <b>nullptr</b>: a missing custom-parameter key yields an empty Collection. Instances of this class are created and consumed by the <see cref='AssetDependencyReplacer'/>.</summary>
class UNIGINE_EDITOR_API AssetDependencyReplacement : public Unigine::APIInterface
{
public:
	typedef const AssetDependencyReplacement ConstAssetDependencyReplacement;
	static constexpr bool HAS_OWNERSHIP = true;

	/// <summary> Returns the GUID of the file whose references this record is used to rewrite. This GUID identifies the file in <a href='https://developer.unigine.com/en/docs/latest/api/library/filesystem/class.filesystem'>Unigine::FileSystem</a>, which resolves it to the file's path and contents.</summary>
	/// <returns> GUID of the file.</returns>
	Unigine::UGUID getFileGUID() const;
	/// <summary> Returns the asset import parameters carried by this record for the file. The method always returns a live <see cref='Collection'/>, never <b>nullptr</b>.</summary>
	/// <returns> Collection of asset import parameters.</returns>
	Unigine::Ptr<Collection> getAssetImportParameters() const;
	/// <summary> Sets the asset import parameters for the file. When a replacer changes the parameters through this method, the dependency replacement system applies the update for the affected assets on the main thread.</summary>
	/// <param name='params'> New collection of asset import parameters to be set.</param>
	void setAssetImportParameters(const Unigine::Ptr<Collection> &params);
	/// <summary> Returns the custom parameters carried by this record for the runtime with the specified file GUID. The method always returns a live <see cref='Collection'/>, never <b>nullptr</b>: a missing key yields an empty Collection.</summary>
	/// <param name='guid'> File GUID of the runtime for which the custom parameters are requested.</param>
	/// <returns> Collection of custom parameters for the runtime with the specified file GUID.</returns>
	Unigine::Ptr<Collection> getCustomParameters(const Unigine::UGUID &guid) const;
	/// <summary> Sets the custom parameters for the runtime with the specified file GUID. When a replacer changes the parameters through this method, the dependency replacement system applies the update for the affected assets on the main thread.</summary>
	/// <param name='guid'> File GUID of the runtime for which the custom parameters are set.</param>
	/// <param name='params'> New collection of custom parameters to be set.</param>
	void setCustomParameters(const Unigine::UGUID &guid, const Unigine::Ptr<Collection> &params);
};
typedef Unigine::Ptr<AssetDependencyReplacement> AssetDependencyReplacementPtr;
typedef Unigine::Ptr<const AssetDependencyReplacement> ConstAssetDependencyReplacementPtr;
typedef const AssetDependencyReplacement ConstAssetDependencyReplacement;

//////////////////////////////////////////////////////////////////////////

/// <summary> This class rewrites references across the project's files, replacing every reference to one file with a reference to another file (for example, to redirect all usages of a deleted texture to a replacement one). It drives built-in replacers for the known file types and lets you register custom replacers per file extension via <see cref='addReplacer'/>. A replacement can be run in a blocking way (<see cref='replaceDependencies'/>, which blocks the calling thread until it completes) or a non-blocking way (<see cref='asyncReplaceDependencies'/>, which returns as fast as possible and reports completion through callbacks). Cancelling does not roll back files that have already been saved.
/// This class works with two distinct kinds of callbacks. A replacer callback, registered per extension via <see cref='addReplacer'/>, runs concurrently on worker threads and rewrites one file's references (same contract as a searcher of the <see cref='FileDependencyWalker'/>). Separately, a non-blocking replacement delivers its result through a completion callback once it finishes; <see cref='asyncReplaceDependencies'/> takes two of these - one delivered on the main thread and one on the worker thread that finishes the replacement.
/// Warning: this class is NOT THREAD-SAFE.</summary>
class UNIGINE_EDITOR_API AssetDependencyReplacer
{
public:
	static bool isInitialized();
	/// <summary> Returns a fresh operation ID that can be passed to the <b>operation_id</b> overloads of the replacement methods. Generating the ID in advance lets you query progress (see <see cref='getAsyncReplaceDependenciesProgress'/>) or cancel (see <see cref='cancelAsyncReplaceDependencies'/>) an asynchronous replacement you are about to start.</summary>
	/// <returns> A fresh unique operation ID.</returns>
	static int generateOperationID();
	/// <summary> Replaces all references to a file with references to another file synchronously. The file being replaced is identified by the given GUIDs - its own file GUID and the GUIDs of its runtimes. The call blocks until the replacement is complete.</summary>
	/// <param name='file_guids_to_replace'> The GUIDs identifying the file whose references are to be replaced - the file GUID and the GUIDs of its runtimes.</param>
	/// <param name='file_guid_to_use'> GUID of the file to reference instead.</param>
	/// <param name='progress'> Optional progress callback invoked on the calling thread with the completion percentage and a pointer to a cancellation flag; writing <b>true</b> to that flag stops processing further assets (files already saved are not rolled back).</param>
	/// <returns> <b>true</b> if the replacement completed successfully; otherwise, <b>false</b>.</returns>
	static bool replaceDependencies(const Unigine::Vector<Unigine::UGUID> &file_guids_to_replace, const Unigine::UGUID &file_guid_to_use, Unigine::CallbackBase2<int, bool *> *progress = nullptr);
	/// <summary> Replaces all references to a file with references to another file synchronously, using a pre-generated operation ID (see <see cref='generateOperationID'/>). The file being replaced is identified by the given GUIDs - its own file GUID and the GUIDs of its runtimes. The call blocks until the replacement is complete.</summary>
	/// <param name='operation_id'> Operation ID identifying this replacement (see <see cref='generateOperationID'/>).</param>
	/// <param name='file_guids_to_replace'> The GUIDs identifying the file whose references are to be replaced - the file GUID and the GUIDs of its runtimes.</param>
	/// <param name='file_guid_to_use'> GUID of the file to reference instead.</param>
	/// <param name='progress'> Optional progress callback invoked on the calling thread with the completion percentage and a pointer to a cancellation flag; writing <b>true</b> to that flag stops processing further assets (files already saved are not rolled back).</param>
	/// <returns> <b>true</b> if the replacement completed successfully; otherwise, <b>false</b>.</returns>
	static bool replaceDependencies(int operation_id, const Unigine::Vector<Unigine::UGUID> &file_guids_to_replace, const Unigine::UGUID &file_guid_to_use, Unigine::CallbackBase2<int, bool *> *progress = nullptr);
	/// <summary> Starts replacing all references to a file with references to another file asynchronously. The file being replaced is identified by the given GUIDs - its own file GUID and the GUIDs of its runtimes. The method is non-blocking and returns as fast as possible; the replacers run on worker threads and completion is reported through the callbacks.</summary>
	/// <param name='callback'> Callback invoked on the main thread with the operation ID when the replacement finishes.</param>
	/// <param name='callback_async'> Callback invoked on the worker thread that finishes the replacement, with the same operation ID. Use it for processing that you want to keep off the main thread; it may be <b>nullptr</b>.</param>
	/// <param name='file_guids_to_replace'> The GUIDs identifying the file whose references are to be replaced - the file GUID and the GUIDs of its runtimes.</param>
	/// <param name='file_guid_to_use'> GUID of the file to reference instead.</param>
	/// <returns> <b>true</b> if the replacement was started successfully; otherwise, <b>false</b>.</returns>
	static bool asyncReplaceDependencies(Unigine::CallbackBase1<int> *callback, Unigine::CallbackBase1<int> *callback_async, const Unigine::Vector<Unigine::UGUID> &file_guids_to_replace, const Unigine::UGUID &file_guid_to_use);
	/// <summary> Starts replacing all references to a file with references to another file asynchronously, using a pre-generated operation ID (see <see cref='generateOperationID'/>). The file being replaced is identified by the given GUIDs - its own file GUID and the GUIDs of its runtimes. The method is non-blocking and returns as fast as possible.</summary>
	/// <param name='operation_id'> Operation ID identifying this replacement (see <see cref='generateOperationID'/>).</param>
	/// <param name='callback'> Callback invoked on the main thread with the operation ID when the replacement finishes.</param>
	/// <param name='callback_async'> Callback invoked on the worker thread that finishes the replacement, with the same operation ID; it may be <b>nullptr</b>.</param>
	/// <param name='file_guids_to_replace'> The GUIDs identifying the file whose references are to be replaced - the file GUID and the GUIDs of its runtimes.</param>
	/// <param name='file_guid_to_use'> GUID of the file to reference instead.</param>
	/// <returns> <b>true</b> if the replacement was started successfully; otherwise, <b>false</b>.</returns>
	static bool asyncReplaceDependencies(int operation_id, Unigine::CallbackBase1<int> *callback, Unigine::CallbackBase1<int> *callback_async, const Unigine::Vector<Unigine::UGUID> &file_guids_to_replace, const Unigine::UGUID &file_guid_to_use);
	/// <summary> Returns the progress of the asynchronous replacement with the specified operation ID.</summary>
	/// <param name='operation_id'> Operation ID of the replacement (see <see cref='generateOperationID'/> and the asynchronous replacement methods).</param>
	/// <returns> Completion percentage of the replacement, in range from 0 to 100.</returns>
	static int getAsyncReplaceDependenciesProgress(int operation_id);
	/// <summary> Cancels the asynchronous replacement with the specified operation ID. The final callback is not delivered and worker threads stop at the next between-assets check (files already saved are not rolled back). The method only requests cancellation and returns immediately; it does not wait for the workers to leave the replacers (see <see cref='cancelAsyncReplaceDependenciesSync'/> for that).</summary>
	/// <param name='operation_id'> Operation ID of the replacement to be cancelled.</param>
	/// <returns> <b>true</b> if a replacement with the specified operation ID was found and cancellation was requested; otherwise, <b>false</b>.</returns>
	static bool cancelAsyncReplaceDependencies(int operation_id);
	/// <summary> Cancels the asynchronous replacement with the specified operation ID and blocks until its worker threads have left the replacers. Unlike <see cref='cancelAsyncReplaceDependencies'/>, which only requests cancellation, this method guarantees that no worker is still executing a replacer when it returns. Call it before unloading a plugin that registered a replacer via <see cref='addReplacer'/>, whose callback code is unmapped on unload.</summary>
	/// <param name='operation_id'> Operation ID of the replacement to be cancelled.</param>
	/// <returns> <b>true</b> if a replacement with the specified operation ID was found and cancelled; otherwise, <b>false</b>.</returns>
	static bool cancelAsyncReplaceDependenciesSync(int operation_id);
	/// <summary> Registers a custom replacer for files with the specified extension. A replacer rewrites one matching file's references from the given GUIDs to the replacement GUID, recording the result in the passed <see cref='AssetDependencyReplacement'/>; during an asynchronous replacement replacers run concurrently on worker threads, so the callback must take this into account (see the class description). The manager takes ownership of the callback and deletes it on removal or destruction. The replacer registry cannot be changed while any replacement is in progress.</summary>
	/// <param name='extension'> File extension the replacer handles (without the leading dot).</param>
	/// <param name='id'> Unique ID of the replacer within the extension, used to remove it later (see <see cref='removeReplacer'/>). The <b>unigine_editor</b> ID is used by the built-in replacers.</param>
	/// <param name='callback'> Replacer callback receiving the <see cref='AssetDependencyReplacement'/> record to fill, the list of file GUIDs to replace, the GUID to use instead, and the operation ID of the replacement that triggered the call.</param>
	/// <returns> <b>true</b> if the replacer is registered successfully; otherwise, <b>false</b> in the following cases:
	/// <ol>
		/// <li>The extension or ID is empty.</li>
		/// <li>A replacer with the same extension and ID is already registered.</li>
		/// <li>A replacement is currently in progress.</li>
	/// </ol>
	/// </returns>
	static bool addReplacer(const char *extension, const char *id, Unigine::CallbackBase4<const Unigine::Ptr<AssetDependencyReplacement> &, const Unigine::Vector<Unigine::UGUID> &, const Unigine::UGUID &, int> *callback);
	/// <summary> Removes the replacer with the specified extension and ID. This works for both custom replacers registered via <see cref='addReplacer'/> and the built-in ones (registered under the <b>unigine_editor</b> ID).</summary>
	/// <param name='extension'> File extension the replacer handles (without the leading dot).</param>
	/// <param name='id'> ID the replacer was registered under.</param>
	/// <returns> <b>true</b> if the replacer is removed successfully; otherwise, <b>false</b>.</returns>
	static bool removeReplacer(const char *extension, const char *id);
	/// <summary> Returns the list of all file extensions for which at least one replacer is registered.</summary>
	/// <returns> The list of supported replacer extensions.</returns>
	static Unigine::Vector<Unigine::String> getSupportedReplacerExtensions();
	/// <summary> Returns the number of file extensions for which at least one replacer is registered.</summary>
	/// <returns> Number of supported replacer extensions.</returns>
	static int getNumSupportedReplacerExtensions();
	/// <summary> Returns the supported replacer extension with the specified index.</summary>
	/// <param name='num'> Index of the extension in range from 0 to the total number of supported extensions (see <see cref='getNumSupportedReplacerExtensions'/>).</param>
	/// <returns> Supported replacer extension with the specified index.</returns>
	static const char *getSupportedReplacerExtension(int num);
	/// <summary> Returns the list of IDs of all replacers registered for the extension with the specified index.</summary>
	/// <param name='extension'> Index of the extension in range from 0 to the total number of supported extensions (see <see cref='getNumSupportedReplacerExtensions'/>).</param>
	/// <returns> The list of replacer IDs registered for the specified extension.</returns>
	static Unigine::Vector<Unigine::String> getReplacerIDs(int extension);
	/// <summary> Returns the number of replacers registered for the extension with the specified index.</summary>
	/// <param name='extension'> Index of the extension in range from 0 to the total number of supported extensions (see <see cref='getNumSupportedReplacerExtensions'/>).</param>
	/// <returns> Number of replacers registered for the specified extension.</returns>
	static int getNumReplacerIDs(int extension);
	/// <summary> Returns the ID of the replacer with the specified index registered for the extension with the specified index.</summary>
	/// <param name='extension'> Index of the extension in range from 0 to the total number of supported extensions (see <see cref='getNumSupportedReplacerExtensions'/>).</param>
	/// <param name='num'> Index of the replacer in range from 0 to the number of replacers registered for the extension (see <see cref='getNumReplacerIDs'/>).</param>
	/// <returns> ID of the replacer with the specified index.</returns>
	static const char *getReplacerID(int extension, int num);
	/// <summary> Returns the list of IDs of all replacers registered for the specified extension.</summary>
	/// <param name='extension'> File extension (without the leading dot).</param>
	/// <returns> The list of replacer IDs registered for the specified extension.</returns>
	static Unigine::Vector<Unigine::String> getReplacerIDs(const char *extension);
	/// <summary> Returns the number of replacers registered for the specified extension.</summary>
	/// <param name='extension'> File extension (without the leading dot).</param>
	/// <returns> Number of replacers registered for the specified extension.</returns>
	static int getNumReplacerIDs(const char *extension);
	/// <summary> Returns the ID of the replacer with the specified index registered for the specified extension.</summary>
	/// <param name='extension'> File extension (without the leading dot).</param>
	/// <param name='num'> Index of the replacer in range from 0 to the number of replacers registered for the extension (see <see cref='getNumReplacerIDs'/>).</param>
	/// <returns> ID of the replacer with the specified index.</returns>
	static const char *getReplacerID(const char *extension, int num);
};

} // namespace UnigineEditor
