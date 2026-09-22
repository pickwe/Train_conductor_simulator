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
#include <UnigineInterpreter.h>
#include <UnigineNode.h>
#include <UniginePtr.h>
#include <UnigineString.h>
#include <UnigineVector.h>

#include "UnigineEditorGlobal.h"
#include "UnigineCollection.h"

namespace UnigineEditor
{


/// <summary> Read/write record describing the dependencies of a single file: its file and asset GUIDs, file type, the runtime files produced from the asset, the GUIDs it references (each with a link type), inherit relations, named variables, and asset import / custom parameters.
/// One record is produced per file by the <see cref='FileDependencyWalker'/>. Built-in searchers fill it for the known file types, and you can register your own searchers via <see cref='FileDependencyWalker::addSearcher'/> to fill records for your own extensions or to extend, modify, or replace what the built-in ones produce - including removing the built-in searchers entirely (see <see cref='FileDependencyWalker::removeSearcher'/>). The records are returned by <see cref='FileDependencyWalker::getDependencies'/> and are also consumed inside the Editor by build, packaging, and cleanup tools and by the dependency UI.
/// Dependencies are collected per file and never transitively. A file under <b>data/</b> has an invisible sibling <b>.meta</b> file holding its import and per-runtime parameters; dependencies described in the <b>.meta</b> are attributed to the main file (for example, the record for <em>data/1.mat</em> holds references from both <em>1.mat</em> and <em>1.mat.meta</em>). Runtime files under <b>data/.runtimes/</b> are independent files: their dependencies are parsed from their own content and attributed to the runtime file itself, not to the owning asset. There is no aggregate per-asset record; building graphs or per-asset aggregation is up to the consuming tools.</summary>
class UNIGINE_EDITOR_API FileDependencies : public Unigine::APIInterface
{
public:
	typedef const FileDependencies ConstFileDependencies;
	static constexpr bool HAS_OWNERSHIP = true;


	/// <summary> File type, describing the role the file plays relative to its asset and runtimes (see the AssetManager class for the asset/runtime model).</summary>
	enum TYPE
	{
		/// <summary> The file is the asset itself - a file in your project that you work with directly in the Asset Browser (as opposed to the runtime files the engine generates from it).</summary>
		TYPE_ASSET = 0,
		/// <summary> The file is the primary runtime of its asset: the representation of the source file in the engine format (e.g. the <em>.texture</em> generated from an <em>image.png</em>, or the <em>.node</em> of an imported <em>.fbx</em>). Only one runtime per asset can be primary.</summary>
		TYPE_RUNTIME_PRIMARY,
		/// <summary> The file is an auxiliary runtime of its asset: a secondary generated file that the primary runtime relies on (e.g. the <em>.mesh</em> accompanying the primary <em>.node</em> of an imported <em>.fbx</em>).</summary>
		TYPE_RUNTIME_AUXILIARY,
		/// <summary> The file is neither an asset nor a generated runtime - any other file the walk encounters that the AssetManager does not track as an asset (e.g. a raw file on disk, or a cluster file that extends a node or world file).</summary>
		TYPE_CUSTOM,
	};

	/// <summary> Kind of a used reference, describing how strongly the file depends on the referenced GUID.</summary>
	enum LINK_TYPE
	{
		/// <summary> A direct link: the reference is required by the file, the engine runtime will not work correctly without it. Direct links come from the file's own content (for example, a <b>&lt;reference&gt;</b> element in an XML file, or a <b>texture0=</b> entry in a material).</summary>
		LINK_TYPE_DIRECT = 0,
		/// <summary> An auxiliary link: an extra reference that is often still needed (for example, to render a correct preview of the file) but is not required for the engine runtime to work. Auxiliary links typically come from the asset's custom parameters (for example, animation curves in a <b>.anim</b> file, or bone overrides in a skinned mesh).</summary>
		LINK_TYPE_AUXILIARY,
	};
	/// <summary> Returns the GUID of the file this record describes. This GUID identifies the file in <a href='https://developer.unigine.com/en/docs/latest/api/library/filesystem/class.filesystem'>Unigine::FileSystem</a>, which resolves it to the file's path and contents.</summary>
	/// <returns> GUID of the file.</returns>
	Unigine::UGUID getFileGUID() const;
	/// <summary> Returns the GUID of the asset this file belongs to. For a file of the <see cref='TYPE::TYPE_ASSET'/> type this is the file's own GUID (equal to <see cref='getFileGUID'/>); for a runtime file it is the GUID of the owning asset; for a file of the <see cref='TYPE::TYPE_CUSTOM'/> type it is empty. This GUID can be used in calls to <a href='https://developer.unigine.com/en/docs/latest/api/library/filesystem/class.filesystem'>Unigine::FileSystem</a>.</summary>
	/// <returns> GUID of the owning asset.</returns>
	Unigine::UGUID getAssetGUID() const;
	/// <summary> Returns the type of the file, describing its role relative to its asset and runtimes (see <see cref='TYPE'/>).</summary>
	/// <returns> File type (see <see cref='TYPE'/>).</returns>
	FileDependencies::TYPE getFileType() const;
	/// <summary> Returns the asset import parameters carried by this record (the values from the asset's <b>.meta</b> file). These import parameters are populated only for a file of the <see cref='TYPE::TYPE_ASSET'/> type; for a runtime or custom file the returned Collection is empty. The method always returns a live <see cref='Collection'/>, never <b>nullptr</b>.</summary>
	/// <returns> Collection of asset import parameters.</returns>
	Unigine::Ptr<Collection> getAssetImportParameters() const;
	/// <summary> Returns the custom parameters carried by this record for the runtime with the specified file GUID. These custom parameters are populated only for a file of the <see cref='TYPE::TYPE_ASSET'/> type; for a runtime or custom file the returned Collection is empty. The method always returns a live <see cref='Collection'/>, never <b>nullptr</b>: a missing key yields an empty Collection.</summary>
	/// <param name='guid'> File GUID of the runtime for which the custom parameters are requested.</param>
	/// <returns> Collection of custom parameters for the runtime with the specified file GUID.</returns>
	Unigine::Ptr<Collection> getCustomParameters(const Unigine::UGUID &guid) const;
	/// <summary> Returns the list of GUIDs of all runtime files produced from the asset this record describes (some assets, like FBX containers, produce multiple runtimes). These runtime file GUIDs are populated only for a file of the <see cref='TYPE::TYPE_ASSET'/> type; for a runtime or custom file the list is empty.</summary>
	/// <returns> The list of runtime file GUIDs.</returns>
	Unigine::Vector<Unigine::UGUID> getRuntimeFileGUIDs() const;
	/// <summary> Returns the number of runtime files produced from the asset this record describes. Runtime files exist only for a file of the <see cref='TYPE::TYPE_ASSET'/> type; for a runtime or custom file the count is 0.</summary>
	/// <returns> Number of runtime file GUIDs.</returns>
	int getNumRuntimeFileGUIDs() const;
	/// <summary> Returns the GUID of the runtime file with the specified index. Runtime files exist only for a file of the <see cref='TYPE::TYPE_ASSET'/> type (see <see cref='getNumRuntimeFileGUIDs'/>).</summary>
	/// <param name='num'> Index of the runtime file in range from 0 to the total number of runtime files (see <see cref='getNumRuntimeFileGUIDs'/>).</param>
	/// <returns> GUID of the runtime file with the specified index.</returns>
	Unigine::UGUID getRuntimeFileGUID(int num) const;
	/// <summary> Returns the list of GUIDs referenced by the file, in content order and index-aligned with the link types (see <see cref='getUsedLinkType'/>). A used GUID may point into different systems: a file GUID resolvable through <a href='https://developer.unigine.com/en/docs/latest/api/library/filesystem/class.filesystem'>Unigine::FileSystem</a>, a material GUID resolvable through Unigine::Materials::findMaterialByGUID, or a property GUID resolvable through Unigine::Properties::findPropertyByGUID; it may also resolve to nothing if the referenced file has been deleted. The same GUID may appear more than once: searchers (including custom ones) keep references as they are found and the walker does not deduplicate them, so consumers that need uniqueness must deduplicate by their own key.</summary>
	/// <returns> The list of used GUIDs.</returns>
	Unigine::Vector<Unigine::UGUID> getUsedGUIDs() const;
	/// <summary> Returns the number of GUIDs referenced by the file.</summary>
	/// <returns> Number of used GUIDs.</returns>
	int getNumUsedGUIDs() const;
	/// <summary> Returns the used GUID with the specified index. The corresponding link type can be obtained via <see cref='getUsedLinkType'/> using the same index.</summary>
	/// <param name='num'> Index of the used GUID in range from 0 to the total number of used GUIDs (see <see cref='getNumUsedGUIDs'/>).</param>
	/// <returns> Used GUID with the specified index.</returns>
	Unigine::UGUID getUsedGUID(int num) const;
	/// <summary> Returns the link type of the used GUID with the specified index (see <see cref='LINK_TYPE'/>).</summary>
	/// <param name='num'> Index of the used GUID in range from 0 to the total number of used GUIDs (see <see cref='getNumUsedGUIDs'/>).</param>
	/// <returns> Link type of the used GUID with the specified index (see <see cref='LINK_TYPE'/>).</returns>
	FileDependencies::LINK_TYPE getUsedLinkType(int num) const;
	/// <summary> Returns a value indicating whether the file references the specified GUID at least once.</summary>
	/// <param name='guid'> GUID to be checked.</param>
	/// <returns> <b>true</b> if the specified GUID is referenced by the file; otherwise, <b>false</b>.</returns>
	bool containsUsedGUID(const Unigine::UGUID &guid) const;
	/// <summary> Adds a used GUID with the specified link type to the record. Intended to be called from a custom searcher (see <see cref='FileDependencyWalker::addSearcher'/>) while it fills the record for a file.</summary>
	/// <param name='guid'> GUID referenced by the file to be added.</param>
	/// <param name='link'> Link type of the reference (see <see cref='LINK_TYPE'/>); defaults to <see cref='LINK_TYPE::LINK_TYPE_DIRECT'/>.</param>
	/// <returns> Index at which the GUID was added.</returns>
	int addUsedGUID(const Unigine::UGUID &guid, FileDependencies::LINK_TYPE link = FileDependencies::LINK_TYPE_DIRECT);
	/// <summary> Removes the used GUID with the specified index (together with its link type).</summary>
	/// <param name='num'> Index of the used GUID to be removed, in range from 0 to the total number of used GUIDs (see <see cref='getNumUsedGUIDs'/>).</param>
	/// <returns> <b>true</b> if the used GUID is removed successfully; otherwise, <b>false</b>.</returns>
	bool removeUsedGUID(int num);
	/// <summary> Returns the inherit ID of the file. Inherit relations describe an inheritance chain between files. The ID is stored as a <b>Variable</b>; in practice it holds a UGUID (for example, a material stores its material's UGUID, and a property stores its property's UGUID).</summary>
	/// <returns> Inherit ID of the file as a <b>Variable</b>.</returns>
	Unigine::Variable  getInheritID() const;
	/// <summary> Sets the inherit ID of the file (see <see cref='getInheritID'/>). Intended to be set by a searcher filling the record.</summary>
	/// <param name='id'> New inherit ID to be set, as a <b>Variable</b> (e.g. a UGUID for materials and properties).</param>
	void setInheritID(const Unigine::Variable &id);
	/// <summary> Returns the inherit ID of the file's parent in the inheritance chain (see <see cref='getInheritID'/>). For materials and properties this is the parent material's or property's UGUID, if the file has a parent; otherwise it is empty.</summary>
	/// <returns> Inherit ID of the parent as a <b>Variable</b>.</returns>
	Unigine::Variable  getInheritParentID() const;
	/// <summary> Sets the inherit ID of the file's parent in the inheritance chain (see <see cref='getInheritID'/>).</summary>
	/// <param name='id'> New parent inherit ID to be set, as a <b>Variable</b>.</param>
	void setInheritParentID(const Unigine::Variable &id);
	/// <summary> Returns the list of inherit IDs of the file's children in the inheritance chain (see <see cref='getInheritID'/>).</summary>
	/// <returns> The list of child inherit IDs, each as a <b>Variable</b>.</returns>
	Unigine::Vector<Unigine::Variable > getInheritChildrenIDs() const;
	/// <summary> Returns the number of the file's children in the inheritance chain.</summary>
	/// <returns> Number of child inherit IDs.</returns>
	int getNumInheritChildrenIDs() const;
	/// <summary> Returns the inherit ID of the file's child with the specified index (see <see cref='getInheritID'/>).</summary>
	/// <param name='num'> Index of the child in range from 0 to the total number of children (see <see cref='getNumInheritChildrenIDs'/>).</param>
	/// <returns> Child inherit ID with the specified index, as a <b>Variable</b>.</returns>
	Unigine::Variable  getInheritChildID(int num) const;
	/// <summary> Adds a child inherit ID to the file's inheritance chain (see <see cref='getInheritID'/>).</summary>
	/// <param name='id'> New child inherit ID to be added, as a <b>Variable</b>.</param>
	/// <returns> Index at which the child inherit ID was added.</returns>
	int addInheritChildID(const Unigine::Variable &id);
	/// <summary> Removes the child inherit ID with the specified index from the file's inheritance chain.</summary>
	/// <param name='num'> Index of the child in range from 0 to the total number of children (see <see cref='getNumInheritChildrenIDs'/>).</param>
	/// <returns> <b>true</b> if the child inherit ID is removed successfully; otherwise, <b>false</b>.</returns>
	bool removeInheritChildID(int num);
	/// <summary> Returns the names of all custom variables stored in the record. Variables let searchers attach arbitrary named data to a file (for example, the node/world per-node breakdown read via <see cref='NodeFileDependencies'/>).</summary>
	/// <returns> The list of variable names.</returns>
	Unigine::Vector<Unigine::String> getVariableNames() const;
	/// <summary> Saves to the vector specified as the argument the names of all custom variables stored in the record (see <see cref='getVariableNames'/>).</summary>
	/// <param name='names'> The vector to store the names of all custom variables stored in the record.</param>
	void getVariableNames(Unigine::Vector<Unigine::String> &names) const;
	/// <summary> Returns the value of the custom variable with the specified name (see <see cref='getVariableNames'/>).</summary>
	/// <param name='name'> Name of the variable.</param>
	/// <returns> Value of the variable as a <b>Variable</b>, or a void <b>Variable</b> if no variable with the specified name exists.</returns>
	Unigine::Variable  getVariable(const char *name) const;
	/// <summary> Returns a value indicating whether a custom variable with the specified name exists in the record.</summary>
	/// <param name='name'> Name of the variable to be checked.</param>
	/// <returns> <b>true</b> if a variable with the specified name exists; otherwise, <b>false</b>.</returns>
	bool hasVariable(const char *name) const;
	/// <summary> Sets the value of the custom variable with the specified name, adding the variable if it does not exist yet.</summary>
	/// <param name='name'> Name of the variable to be set.</param>
	/// <param name='value'> New value to be set, as a <b>Variable</b>.</param>
	void setVariable(const char *name, const Unigine::Variable &value);
	/// <summary> Removes the custom variable with the specified name from the record.</summary>
	/// <param name='name'> Name of the variable to be removed.</param>
	void removeVariable(const char *name);
};
typedef Unigine::Ptr<FileDependencies> FileDependenciesPtr;
typedef Unigine::Ptr<const FileDependencies> ConstFileDependenciesPtr;
typedef const FileDependencies ConstFileDependencies;


/// <summary> A strongly-typed view over the per-node breakdown of a node or world <see cref='FileDependencies'/> record. For each node in the asset it exposes the node's ID, type, and name, and the file GUIDs that node uses.
/// The breakdown is stored inside the <see cref='FileDependencies'/> record as a named variable (see <see cref='getVariableName'/> and <see cref='FileDependencies::getVariable'/>); this class decodes that variable into a convenient form.</summary>
class UNIGINE_EDITOR_API NodeFileDependencies : public Unigine::APIInterface
{
public:
	typedef const NodeFileDependencies ConstNodeFileDependencies;
	static constexpr bool HAS_OWNERSHIP = true;

	/// <summary> Returns the name of the <see cref='FileDependencies'/> variable under which the per-node breakdown is stored. Use it with <see cref='FileDependencies::getVariable'/> to fetch the raw value, or pass a <see cref='FileDependencies'/> directly to <see cref='create'/>.</summary>
	/// <returns> Name of the variable holding the per-node breakdown.</returns>
	static const char *getVariableName();
	/// <summary> Creates a node-dependencies view from the specified node or world <see cref='FileDependencies'/> record. Returns an empty view if the record holds no per-node breakdown.</summary>
	/// <param name='file_dependencies'> Node or world file-dependencies record to read the per-node breakdown from.</param>
	/// <returns> A new <see cref='NodeFileDependencies'/> view.</returns>
	static Unigine::Ptr<NodeFileDependencies> create(const Unigine::Ptr<FileDependencies> &file_dependencies);
	/// <summary> Creates a node-dependencies view from the raw variable holding the per-node breakdown (the value stored under <see cref='getVariableName'/>).</summary>
	/// <param name='variable'> Variable holding the encoded per-node breakdown.</param>
	/// <returns> A new <see cref='NodeFileDependencies'/> view.</returns>
	static Unigine::Ptr<NodeFileDependencies> create(const Unigine::Variable & variable);
	/// <summary> Returns the number of nodes in the breakdown.</summary>
	/// <returns> Number of nodes.</returns>
	int getNumNodes() const;
	/// <summary> Returns the ID of the node with the specified index.</summary>
	/// <param name='num'> Index of the node in range from 0 to the total number of nodes (see <see cref='getNumNodes'/>).</param>
	/// <returns> ID of the node with the specified index.</returns>
	int getNodeID(int num) const;
	/// <summary> Returns the name of the node with the specified index.</summary>
	/// <param name='num'> Index of the node in range from 0 to the total number of nodes (see <see cref='getNumNodes'/>).</param>
	/// <returns> Name of the node with the specified index.</returns>
	const char *getNodeName(int num) const;
	/// <summary> Returns the type of the node with the specified index (see <a href='https://developer.unigine.com/en/docs/latest/api/library/nodes/class.node'>Node::TYPE</a>).</summary>
	/// <param name='num'> Index of the node in range from 0 to the total number of nodes (see <see cref='getNumNodes'/>).</param>
	/// <returns> Type of the node with the specified index (see <a href='https://developer.unigine.com/en/docs/latest/api/library/nodes/class.node'>Node::TYPE</a>).</returns>
	Unigine::Node::TYPE getNodeType(int num) const;
	/// <summary> Returns the list of file GUIDs used by the node with the specified index.</summary>
	/// <param name='node'> Index of the node in range from 0 to the total number of nodes (see <see cref='getNumNodes'/>).</param>
	/// <returns> The list of file GUIDs used by the specified node.</returns>
	Unigine::Vector<Unigine::UGUID> getNodeUsedGUIDs(int node) const;
	/// <summary> Returns the number of file GUIDs used by the node with the specified index.</summary>
	/// <param name='node'> Index of the node in range from 0 to the total number of nodes (see <see cref='getNumNodes'/>).</param>
	/// <returns> Number of file GUIDs used by the specified node.</returns>
	int getNumNodeUsedGUIDs(int node) const;
	/// <summary> Returns the used file GUID with the specified index for the node with the specified index.</summary>
	/// <param name='node'> Index of the node in range from 0 to the total number of nodes (see <see cref='getNumNodes'/>).</param>
	/// <param name='num'> Index of the used GUID in range from 0 to the total number of GUIDs used by the node (see <see cref='getNumNodeUsedGUIDs'/>).</param>
	/// <returns> Used file GUID with the specified index for the specified node.</returns>
	Unigine::UGUID getNodeUsedGUID(int node, int num) const;
};
typedef Unigine::Ptr<NodeFileDependencies> NodeFileDependenciesPtr;
typedef Unigine::Ptr<const NodeFileDependencies> ConstNodeFileDependenciesPtr;
typedef const NodeFileDependencies ConstNodeFileDependencies;

//////////////////////////////////////////////////////////////////////////

/// <summary> This class walks the project's files and collects their dependencies, producing one <see cref='FileDependencies'/> record per file. It drives built-in searchers for the known file types and lets you register custom searchers per file extension via <see cref='addSearcher'/>. A walk can be run in a blocking way (<see cref='getDependencies'/> / <see cref='getAllDependencies'/>, which block the calling thread until the walk completes) or a non-blocking way (<see cref='asyncGetDependencies'/> / <see cref='asyncGetAllDependencies'/>, which return as fast as possible and deliver the result through callbacks).
/// This class works with two distinct kinds of callbacks. A searcher callback, registered per extension via <see cref='addSearcher'/>, runs concurrently on worker threads and fills one file's record; it must be thread-safe (keep per-call state on the stack, capture only read-only shared state). Separately, an asynchronous walk delivers its result through a completion callback once the walk finishes; <see cref='asyncGetDependencies'/> / <see cref='asyncGetAllDependencies'/> take two of these - one delivered on the main thread and one on the worker thread that finishes the walk.
/// Warning: this class is NOT THREAD-SAFE.</summary>
class UNIGINE_EDITOR_API FileDependencyWalker
{
public:
	static bool isInitialized();
	/// <summary> Returns a fresh operation ID that can be passed to the <b>operation_id</b> overloads of the walk methods. Generating the ID in advance lets you query progress (see <see cref='getAsyncGetDependenciesProgress'/>) or cancel (see <see cref='cancelAsyncGetDependencies'/>) an asynchronous walk you are about to start.</summary>
	/// <returns> A fresh unique operation ID.</returns>
	static int generateOperationID();
	/// <summary> Collects the dependencies of the specified files in a blocking way: the call blocks the calling thread until the walk completes, while the searchers run on worker threads. Together with each given file, the walk also covers the runtime files it owns: when a GUID points to an asset, the asset's source file and all of its generated runtimes are walked too.</summary>
	/// <param name='file_guids'> The list of file GUIDs to walk.</param>
	/// <param name='progress'> Optional progress callback invoked on the calling thread with the completion percentage and a pointer to a cancellation flag; writing <b>true</b> to that flag aborts the walk early, in which case an empty result is returned.</param>
	/// <returns> The list of <see cref='FileDependencies'/> records, one per walked file.</returns>
	static Unigine::Vector<Unigine::Ptr<FileDependencies>> getDependencies(const Unigine::Vector<Unigine::UGUID> &file_guids, Unigine::CallbackBase2<int, bool *> *progress = nullptr);
	/// <summary> Collects the dependencies of the specified files in a blocking way, using a pre-generated operation ID (see <see cref='generateOperationID'/>): the call blocks the calling thread until the walk completes, while the searchers run on worker threads. Together with each given file, the walk also covers the runtime files it owns: when a GUID points to an asset, the asset's source file and all of its generated runtimes are walked too.</summary>
	/// <param name='operation_id'> Operation ID identifying this walk (see <see cref='generateOperationID'/>).</param>
	/// <param name='file_guids'> The list of file GUIDs to walk.</param>
	/// <param name='progress'> Optional progress callback invoked on the calling thread with the completion percentage and a pointer to a cancellation flag; writing <b>true</b> to that flag aborts the walk early, in which case an empty result is returned.</param>
	/// <returns> The list of <see cref='FileDependencies'/> records, one per walked file.</returns>
	static Unigine::Vector<Unigine::Ptr<FileDependencies>> getDependencies(int operation_id, const Unigine::Vector<Unigine::UGUID> &file_guids, Unigine::CallbackBase2<int, bool *> *progress = nullptr);
	/// <summary> Collects the dependencies of every file in the project in a blocking way: the call blocks the calling thread until the walk completes, while the searchers run on worker threads.</summary>
	/// <param name='progress'> Optional progress callback invoked on the calling thread with the completion percentage and a pointer to a cancellation flag; writing <b>true</b> to that flag aborts the walk early, in which case an empty result is returned.</param>
	/// <returns> The list of <see cref='FileDependencies'/> records, one per file in the project.</returns>
	static Unigine::Vector<Unigine::Ptr<FileDependencies>> getAllDependencies(Unigine::CallbackBase2<int, bool *> *progress = nullptr);
	/// <summary> Collects the dependencies of every file in the project in a blocking way, using a pre-generated operation ID (see <see cref='generateOperationID'/>): the call blocks the calling thread until the walk completes, while the searchers run on worker threads.</summary>
	/// <param name='operation_id'> Operation ID identifying this walk (see <see cref='generateOperationID'/>).</param>
	/// <param name='progress'> Optional progress callback invoked on the calling thread with the completion percentage and a pointer to a cancellation flag; writing <b>true</b> to that flag aborts the walk early, in which case an empty result is returned.</param>
	/// <returns> The list of <see cref='FileDependencies'/> records, one per file in the project.</returns>
	static Unigine::Vector<Unigine::Ptr<FileDependencies>> getAllDependencies(int operation_id, Unigine::CallbackBase2<int, bool *> *progress = nullptr);
	/// <summary> Starts collecting the dependencies of the specified files (and the runtimes they own) asynchronously. The method is non-blocking and returns as fast as possible; the searchers run on worker threads and the result is delivered through the callbacks.</summary>
	/// <param name='callback'> Callback invoked on the main thread with the resulting list of <see cref='FileDependencies'/> records when the walk finishes.</param>
	/// <param name='callback_async'> Callback invoked on the worker thread that finishes the walk, with the same resulting list. Use it for processing that you want to keep off the main thread; it may be <b>nullptr</b>.</param>
	/// <param name='file_guids'> The list of file GUIDs to walk.</param>
	/// <returns> The operation ID of the started walk, which can be passed to <see cref='getAsyncGetDependenciesProgress'/>, <see cref='cancelAsyncGetDependencies'/>, or <see cref='cancelAsyncGetDependenciesSync'/>.</returns>
	static int asyncGetDependencies(Unigine::CallbackBase1<const Unigine::Vector<Unigine::Ptr<FileDependencies>> &> *callback, Unigine::CallbackBase1<const Unigine::Vector<Unigine::Ptr<FileDependencies>> &> *callback_async, const Unigine::Vector<Unigine::UGUID> &file_guids);
	/// <summary> Starts collecting the dependencies of the specified files (and the runtimes they own) asynchronously, using a pre-generated operation ID (see <see cref='generateOperationID'/>). The method is non-blocking and returns as fast as possible.</summary>
	/// <param name='operation_id'> Operation ID identifying this walk (see <see cref='generateOperationID'/>).</param>
	/// <param name='callback'> Callback invoked on the main thread with the resulting list of <see cref='FileDependencies'/> records when the walk finishes.</param>
	/// <param name='callback_async'> Callback invoked on the worker thread that finishes the walk, with the same resulting list; it may be <b>nullptr</b>.</param>
	/// <param name='file_guids'> The list of file GUIDs to walk.</param>
	/// <returns> The operation ID of the started walk (the same value passed as <b>operation_id</b>).</returns>
	static int asyncGetDependencies(int operation_id, Unigine::CallbackBase1<const Unigine::Vector<Unigine::Ptr<FileDependencies>> &> *callback, Unigine::CallbackBase1<const Unigine::Vector<Unigine::Ptr<FileDependencies>> &> *callback_async, const Unigine::Vector<Unigine::UGUID> &file_guids);
	/// <summary> Starts collecting the dependencies of every file in the project asynchronously. The method is non-blocking and returns as fast as possible; the searchers run on worker threads and the result is delivered through the callbacks.</summary>
	/// <param name='callback'> Callback invoked on the main thread with the resulting list of <see cref='FileDependencies'/> records when the walk finishes.</param>
	/// <param name='callback_async'> Callback invoked on the worker thread that finishes the walk, with the same resulting list; it may be <b>nullptr</b>.</param>
	/// <returns> The operation ID of the started walk, which can be passed to <see cref='getAsyncGetDependenciesProgress'/>, <see cref='cancelAsyncGetDependencies'/>, or <see cref='cancelAsyncGetDependenciesSync'/>.</returns>
	static int asyncGetAllDependencies(Unigine::CallbackBase1<const Unigine::Vector<Unigine::Ptr<FileDependencies>> &> *callback, Unigine::CallbackBase1<const Unigine::Vector<Unigine::Ptr<FileDependencies>> &> *callback_async);
	/// <summary> Starts collecting the dependencies of every file in the project asynchronously, using a pre-generated operation ID (see <see cref='generateOperationID'/>). The method is non-blocking and returns as fast as possible.</summary>
	/// <param name='operation_id'> Operation ID identifying this walk (see <see cref='generateOperationID'/>).</param>
	/// <param name='callback'> Callback invoked on the main thread with the resulting list of <see cref='FileDependencies'/> records when the walk finishes.</param>
	/// <param name='callback_async'> Callback invoked on the worker thread that finishes the walk, with the same resulting list; it may be <b>nullptr</b>.</param>
	/// <returns> The operation ID of the started walk (the same value passed as <b>operation_id</b>).</returns>
	static int asyncGetAllDependencies(int operation_id, Unigine::CallbackBase1<const Unigine::Vector<Unigine::Ptr<FileDependencies>> &> *callback, Unigine::CallbackBase1<const Unigine::Vector<Unigine::Ptr<FileDependencies>> &> *callback_async);
	/// <summary> Returns the progress of the asynchronous walk with the specified operation ID. After the walk is cancelled the last reported value keeps being returned.</summary>
	/// <param name='operation_id'> Operation ID of the walk (see <see cref='generateOperationID'/> and the asynchronous walk methods).</param>
	/// <returns> Completion percentage of the walk, in range from 0 to 100.</returns>
	static int getAsyncGetDependenciesProgress(int operation_id);
	/// <summary> Cancels the asynchronous walk with the specified operation ID. The final callback is not delivered and worker threads stop at the next between-files check. The method only requests cancellation and returns immediately; it does not wait for the workers to leave the searchers (see <see cref='cancelAsyncGetDependenciesSync'/> for that).</summary>
	/// <param name='operation_id'> Operation ID of the walk to be cancelled.</param>
	/// <returns> <b>true</b> if a walk with the specified operation ID was found and cancellation was requested; otherwise, <b>false</b>.</returns>
	static bool cancelAsyncGetDependencies(int operation_id);
	/// <summary> Cancels the asynchronous walk with the specified operation ID and blocks until its worker threads have left the searchers. Unlike <see cref='cancelAsyncGetDependencies'/>, which only requests cancellation, this method guarantees that no worker is still executing a searcher when it returns. Call it before unloading a plugin that registered a searcher via <see cref='addSearcher'/>, whose callback code is unmapped on unload.</summary>
	/// <param name='operation_id'> Operation ID of the walk to be cancelled.</param>
	/// <returns> <b>true</b> if a walk with the specified operation ID was found and cancelled; otherwise, <b>false</b>.</returns>
	static bool cancelAsyncGetDependenciesSync(int operation_id);
	/// <summary> Registers a custom searcher for files with the specified extension. A searcher fills the passed <see cref='FileDependencies'/> record (used GUIDs, inherit relations, variables) for one matching file; during an asynchronous walk searchers run concurrently on worker threads, so the callback must take this into account (see the class description). The manager takes ownership of the callback and deletes it on removal or destruction. The searcher registry cannot be changed while any walk is in progress.</summary>
	/// <param name='extension'> File extension the searcher handles (without the leading dot).</param>
	/// <param name='id'> Unique ID of the searcher within the extension, used to remove it later (see <see cref='removeSearcher'/>). The <b>unigine_editor</b> ID is used by the built-in searchers.</param>
	/// <param name='callback'> Searcher callback receiving the <see cref='FileDependencies'/> record to fill and the operation ID of the walk that triggered the call.</param>
	/// <returns> <b>true</b> if the searcher is registered successfully; otherwise, <b>false</b> in the following cases:
	/// <ol>
		/// <li>The extension or ID is empty.</li>
		/// <li>A searcher with the same extension and ID is already registered.</li>
		/// <li>A dependency walk is currently in progress.</li>
	/// </ol>
	/// </returns>
	static bool addSearcher(const char *extension, const char *id, Unigine::CallbackBase2<const Unigine::Ptr<FileDependencies> &, int> *callback);
	/// <summary> Removes the searcher with the specified extension and ID. This works for both custom searchers registered via <see cref='addSearcher'/> and the built-in ones (registered under the <b>unigine_editor</b> ID).</summary>
	/// <param name='extension'> File extension the searcher handles (without the leading dot).</param>
	/// <param name='id'> ID the searcher was registered under.</param>
	/// <returns> <b>true</b> if the searcher is removed successfully; otherwise, <b>false</b>.</returns>
	static bool removeSearcher(const char *extension, const char *id);
	/// <summary> Returns the list of all file extensions for which at least one searcher is registered.</summary>
	/// <returns> The list of supported searcher extensions.</returns>
	static Unigine::Vector<Unigine::String> getSupportedSearcherExtensions();
	/// <summary> Returns the number of file extensions for which at least one searcher is registered.</summary>
	/// <returns> Number of supported searcher extensions.</returns>
	static int getNumSupportedSearcherExtensions();
	/// <summary> Returns the supported searcher extension with the specified index.</summary>
	/// <param name='num'> Index of the extension in range from 0 to the total number of supported extensions (see <see cref='getNumSupportedSearcherExtensions'/>).</param>
	/// <returns> Supported searcher extension with the specified index.</returns>
	static const char *getSupportedSearcherExtension(int num);
	/// <summary> Returns the list of IDs of all searchers registered for the extension with the specified index.</summary>
	/// <param name='extension'> Index of the extension in range from 0 to the total number of supported extensions (see <see cref='getNumSupportedSearcherExtensions'/>).</param>
	/// <returns> The list of searcher IDs registered for the specified extension.</returns>
	static Unigine::Vector<Unigine::String> getSearcherIDs(int extension);
	/// <summary> Returns the number of searchers registered for the extension with the specified index.</summary>
	/// <param name='extension'> Index of the extension in range from 0 to the total number of supported extensions (see <see cref='getNumSupportedSearcherExtensions'/>).</param>
	/// <returns> Number of searchers registered for the specified extension.</returns>
	static int getNumSearcherIDs(int extension);
	/// <summary> Returns the ID of the searcher with the specified index registered for the extension with the specified index.</summary>
	/// <param name='extension'> Index of the extension in range from 0 to the total number of supported extensions (see <see cref='getNumSupportedSearcherExtensions'/>).</param>
	/// <param name='num'> Index of the searcher in range from 0 to the number of searchers registered for the extension (see <see cref='getNumSearcherIDs'/>).</param>
	/// <returns> ID of the searcher with the specified index.</returns>
	static const char *getSearcherID(int extension, int num);
	/// <summary> Returns the list of IDs of all searchers registered for the specified extension.</summary>
	/// <param name='extension'> File extension (without the leading dot).</param>
	/// <returns> The list of searcher IDs registered for the specified extension.</returns>
	static Unigine::Vector<Unigine::String> getSearcherIDs(const char *extension);
	/// <summary> Returns the number of searchers registered for the specified extension.</summary>
	/// <param name='extension'> File extension (without the leading dot).</param>
	/// <returns> Number of searchers registered for the specified extension.</returns>
	static int getNumSearcherIDs(const char *extension);
	/// <summary> Returns the ID of the searcher with the specified index registered for the specified extension.</summary>
	/// <param name='extension'> File extension (without the leading dot).</param>
	/// <param name='num'> Index of the searcher in range from 0 to the number of searchers registered for the extension (see <see cref='getNumSearcherIDs'/>).</param>
	/// <returns> ID of the searcher with the specified index.</returns>
	static const char *getSearcherID(const char *extension, int num);
};

} // namespace UnigineEditor
