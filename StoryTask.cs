﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

#if NETWORKED

using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

#endif

public enum TASKTYPE
{

	// is END still a thign?
	BASIC,
	ROUTING,
	END

}

public enum TASKSTATUS
{
	ACTIVE,
	COMPLETE,

}

public class StoryTask
{
	
	string me = "Storytask";
	public string ID;
	int signoffs;
	public string description;
	TASKSTATUS status;
	public StoryPointer pointer;
	public SCOPE scope;

	public float startTime, duration;
	public float d;

	public Dictionary<string,Int32> taskIntValues;
	public Dictionary<string,float> taskFloatValues;
	public Dictionary<string,Quaternion> taskQuaternionValues;
	public Dictionary<string,Vector3> taskVector3Values;
	public Dictionary<string,string> taskStringValues;
	public Dictionary<string,ushort[]> taskUshortValues;




	#if NETWORKED

	public Dictionary<string,bool> taskValuesChangeMask;
	List<string> changedTaskValue;
	public bool modified = false;
	bool allModified = false;
	#endif


	public void markAllAsModified ()
	{
		allModified = true;
		modified = true;
	}

	public StoryTask ()
	{

		// a minimal version with just data. no references. used for data carry-over task. 

		pointer = new StoryPointer ();

		setDefaults ();

	}

	public StoryTask (string myDescription, StoryPointer fromStoryPointer, string setID)
	{

		// Task created from network, so global scope and with passed in ID.

		description = myDescription;
		pointer = fromStoryPointer;
		ID = setID;
		scope = SCOPE.GLOBAL;

		setDefaults ();
		GENERAL.ALLTASKS.Add (this);
	}

	public StoryTask (StoryPointer fromStoryPointer, SCOPE setScope)
	{

		// Create a task based on the current storypoint of the pointer.
		// Note that setting scope is explicit, but in effect the scope of the task is the same as the scope of the pointer.

		description = fromStoryPointer.currentPoint.task [0];

		pointer = fromStoryPointer;
		ID = UUID.getGlobalID ();
		scope = setScope;

		setDefaults ();
		GENERAL.ALLTASKS.Add (this);
	}

	void setDefaults ()
	{

		signoffs = 0;

		//		GENERAL.ALLTASKS.Add (this);

		taskIntValues = new Dictionary<string,int> ();
		taskFloatValues = new Dictionary<string,float> ();
		taskQuaternionValues = new Dictionary<string,Quaternion> ();
		taskVector3Values = new Dictionary<string,Vector3> ();
		taskStringValues = new Dictionary<string,string> ();

		taskUshortValues = new  Dictionary<string,ushort[]> ();


		//		taskIntValues ["status"] = (Int32) TASKSTATUS.ACTIVE;

		#if NETWORKED
		taskValuesChangeMask = new Dictionary<string,bool> ();
		#endif

		setStatus (TASKSTATUS.ACTIVE);

		//		taskIntValues ["status"] = (Int32)TASKSTATUS.ACTIVE;

		//		taskValuesChangeMask ["status"] = false;

	}

	public void loadPersistantData (StoryPointer referencePointer)
	{

		// we use the update message internally to transfer values from the carry over task

		if (referencePointer.scope == SCOPE.GLOBAL) {

			// mark changemask as true so these values get distributed over the network.

			ApplyUpdateMessage (referencePointer.persistantData.getUpdateMessage (), true);


		} else {

			ApplyUpdateMessage (referencePointer.persistantData.getUpdateMessage ());

		}

	}

	#if NETWORKED

	public TaskUpdate getUpdateMessage ()
	{

		TaskUpdate msg = new TaskUpdate ();

		msg.pointerID = pointer.ID;
		msg.taskID = ID;
		msg.description = description;

		msg.updatedIntNames = new List<string> ();
		msg.updatedIntValues = new List<Int32> ();
		msg.updatedFloatNames = new List<string> ();
		msg.updatedFloatValues = new List<float> ();
		msg.updatedQuaternionNames = new List<string> ();
		msg.updatedQuaternionValues = new List<Quaternion> ();
		msg.updatedVector3Names = new List<string> ();
		msg.updatedVector3Values = new List<Vector3> ();
		msg.updatedStringNames = new List<string> ();
		msg.updatedStringValues = new List<string> ();

		msg.updatedUshortNames = new List<string> ();
		msg.updatedUshortValues = new List<ushort[]> ();



		string[] intNames = taskIntValues.Keys.ToArray ();

		foreach (string intName in intNames) {

			if (taskValuesChangeMask [intName] || allModified) {

				msg.updatedIntNames.Add (intName);

				taskValuesChangeMask [intName] = false;

				int intValue;

				if (taskIntValues.TryGetValue (intName, out intValue))
					msg.updatedIntValues.Add (intValue);

			}

		}

		string[] floatNames = taskFloatValues.Keys.ToArray ();

		foreach (string floatName in floatNames) {

			if (taskValuesChangeMask [floatName] || allModified) {

				msg.updatedFloatNames.Add (floatName);

				taskValuesChangeMask [floatName] = false;

				float floatValue;

				if (taskFloatValues.TryGetValue (floatName, out floatValue))
					msg.updatedFloatValues.Add (floatValue);

			}

		}

		string[] quaternionNames = taskQuaternionValues.Keys.ToArray ();

		foreach (string quaternionName in quaternionNames) {

			if (taskValuesChangeMask [quaternionName] || allModified) {

				msg.updatedQuaternionNames.Add (quaternionName);

				taskValuesChangeMask [quaternionName] = false;

				Quaternion quaternionValue;

				if (taskQuaternionValues.TryGetValue (quaternionName, out quaternionValue))
					msg.updatedQuaternionValues.Add (quaternionValue);

			}

		}

		string[] vector3Names = taskVector3Values.Keys.ToArray ();

		foreach (string vector3Name in vector3Names) {

			if (taskValuesChangeMask [vector3Name] || allModified) {

				msg.updatedVector3Names.Add (vector3Name);

				taskValuesChangeMask [vector3Name] = false;

				Vector3 vector3Value;

				if (taskVector3Values.TryGetValue (vector3Name, out vector3Value))
					msg.updatedVector3Values.Add (vector3Value);

			}

		}

		string[] stringNames = taskStringValues.Keys.ToArray ();

		foreach (string stringName in stringNames) {

			if (taskValuesChangeMask [stringName] || allModified) {

				msg.updatedStringNames.Add (stringName);

				taskValuesChangeMask [stringName] = false;

				string stringValue;

				if (taskStringValues.TryGetValue (stringName, out stringValue))
					msg.updatedStringValues.Add (stringValue);

			}

		}

		string[] ushortNames = taskUshortValues.Keys.ToArray ();

		foreach (string ushortName in ushortNames) {

			if (taskValuesChangeMask [ushortName] || allModified) {

				msg.updatedUshortNames.Add (ushortName);

				taskValuesChangeMask [ushortName] = false;

				ushort[] ushortValue;

				if (taskUshortValues.TryGetValue (ushortName, out ushortValue))
					msg.updatedUshortValues.Add (ushortValue);

			}

		}

		allModified = false;

		return msg;

	}

	public void ApplyUpdateMessage (TaskUpdate update, bool changeMask = false)
	{

		//		Log.Message ("Applying network task update.");


		if (update.updatedIntNames.Contains ("status")) {
			Log.Message ("incoming task status change, setting pointerstatus to taskupdated.",me,LOGLEVEL.VERBOSE);

			pointer.setStatus (POINTERSTATUS.TASKUPDATED);
		}


		// check task status change

		//		if (update.updatedIntNames ["status"] ) {
		//			Log.Message ("incoming task status change.");
		//			pointer.setStatus (POINTERSTATUS.TASKUPDATED);
		//
		//
		//		}

		for (int i = 0; i < update.updatedIntNames.Count; i++) {
			taskIntValues [update.updatedIntNames [i]] = update.updatedIntValues [i];
			taskValuesChangeMask [update.updatedIntNames [i]] = changeMask;

		}

		for (int i = 0; i < update.updatedFloatNames.Count; i++) {
			taskFloatValues [update.updatedFloatNames [i]] = update.updatedFloatValues [i];
			taskValuesChangeMask [update.updatedFloatNames [i]] = changeMask;

		}

		for (int i = 0; i < update.updatedQuaternionNames.Count; i++) {
			taskQuaternionValues [update.updatedQuaternionNames [i]] = update.updatedQuaternionValues [i];
			taskValuesChangeMask [update.updatedQuaternionNames [i]] = changeMask;

		}

		for (int i = 0; i < update.updatedVector3Names.Count; i++) {
			taskVector3Values [update.updatedVector3Names [i]] = update.updatedVector3Values [i];
			taskValuesChangeMask [update.updatedVector3Names [i]] = changeMask;

		}

		for (int i = 0; i < update.updatedStringNames.Count; i++) {
			taskStringValues [update.updatedStringNames [i]] = update.updatedStringValues [i];
			taskValuesChangeMask [update.updatedStringNames [i]] = changeMask;

		}

		for (int i = 0; i < update.updatedUshortNames.Count; i++) {
			
			taskUshortValues [update.updatedUshortNames [i]] = update.updatedUshortValues [i];
			taskValuesChangeMask [update.updatedUshortNames [i]] = changeMask;

		}





	}

	#endif

	public void setIntValue (string valueName, Int32 value)
	{

		taskIntValues [valueName] = value;

		#if NETWORKED
		taskValuesChangeMask [valueName] = true;
		modified = true;
		#endif

	}

	public bool getIntValue (string valueName, out Int32 value)
	{

		if (!taskIntValues.TryGetValue (valueName, out value)) {
			return false;
		}

		return true;

	}

	public void setStringValue (string valueName, string value)
	{

		taskStringValues [valueName] = value;

		#if NETWORKED
		taskValuesChangeMask [valueName] = true;
		modified = true;
		#endif

	}

	public bool getStringValue (string valueName, out string value)
	{

		if (!taskStringValues.TryGetValue (valueName, out value)) {
			return false;
		}

		return true;

	}

	public void setFloatValue (string valueName, float value)
	{

		taskFloatValues [valueName] = value;

		#if NETWORKED
		taskValuesChangeMask [valueName] = true;
		modified = true;
		#endif

	}

	public bool getFloatValue (string valueName, out float value)
	{

		if (!taskFloatValues.TryGetValue (valueName, out value)) {
			return false;
		}

		return true;

	}

	public void setUshortValue (string valueName, ushort[] value)
	{

		taskUshortValues [valueName] = value;

		#if NETWORKED
		taskValuesChangeMask [valueName] = true;
		modified = true;
		#endif

	}

	public bool getUshortValue (string valueName, out ushort[] value)
	{

		if (!taskUshortValues.TryGetValue (valueName, out value)) {
			return false;
		}

		return true;

	}

	//	public void setStringValue (string valueName, string value)
	//	{
	//
	//		taskStringValues [valueName] = value;
	//
	//		#if NETWORKED
	//		taskValuesChangeMask [valueName] = true;
	//		hasChanged = true;
	//		#endif
	//
	//	}
	//
	//	public bool getStringValue (string valueName, out string value)
	//	{
	//
	//		if (!taskStringValues.TryGetValue (valueName, out value)) {
	//			return false;
	//		}
	//
	//		return true;
	//
	//	}

	public void setVector3Value (string valueName, Vector3 value)
	{

		taskVector3Values [valueName] = value;

		#if NETWORKED
		taskValuesChangeMask [valueName] = true;
		modified = true;
		#endif

	}

	public bool getVector3Value (string valueName, out Vector3 value)
	{

		if (!taskVector3Values.TryGetValue (valueName, out value)) {
			return false;
		}

		return true;

	}

	public void setQuaternionValue (string valueName, Quaternion value)
	{

		taskQuaternionValues [valueName] = value;

		#if NETWORKED
		taskValuesChangeMask [valueName] = true;
		modified = true;
		#endif

	}

	public bool getQuaternionValue (string valueName, out Quaternion value)
	{

		if (!taskQuaternionValues.TryGetValue (valueName, out value)) {
			return false;
		}

		return true;

	}



	//	public void setIntValue (string valueName, Int32 value)
	//
	//	{
	//
	//		taskIntValues [valueName] = value;
	//
	//		#if NETWORKED
	//		taskValuesChangeMask [valueName] = true;
	//		hasChanged = true;
	//		#endif
	//
	//	}
	//
	//	public bool getIntValue (string valueName, out Int32 value)
	//
	//	{
	//
	//		if (!taskIntValues.TryGetValue (valueName, out value)) {
	//			return false;
	//		}
	//
	//		return true;
	//
	//	}


	void setPointerToUpdated ()
	{

		switch (GENERAL.AUTHORITY) {

		case AUTHORITY.GLOBAL:
			//		case SCOPE.SOLO:

			// we're the global server or running solo so we can trigger the pointer. regardless of the task's scope.

			pointer.setStatus (POINTERSTATUS.TASKUPDATED);

			break;

		case AUTHORITY.LOCAL:

			// we're a local client. only if the task is local do we trigger the pointer.

			if (scope == SCOPE.LOCAL) {

				pointer.setStatus (POINTERSTATUS.TASKUPDATED);

			}

			break;

		default:


			break;



		}



	}

	//	bool haveAuthority ()
	//	{
	//
	//		if (scope == SCOPE.LOCAL) {
	//
	//			// Task is local so we have authority
	//
	//			return true;
	//
	//		} else {
	//
	//			if (GENERAL.SCOPE == SCOPE.LOCAL) {
	//
	//				// Global task but local scope.
	//
	//				return false;
	//
	//
	//			} else {
	//
	//				return true;
	//
	//			}
	//
	//
	//		}
	//
	//
	//
	//	}


	public void setStatus (TASKSTATUS theStatus)
	{

		status = theStatus;

		//		taskIntValues ["status"] = (Int32)theStatus;

		setPointerToUpdated ();



		//		#if NETWORKED
		//
		//		taskValuesChangeMask ["status"] = true;
		//		hasChanged = true;
		//
		//		#endif




		//		switch (GENERAL.SCOPE) {
		//
		//		case SCOPE.GLOBAL:
		//		case SCOPE.SOLO:
		//			// we're the global server or running solo so we can trigger the pointer. regardless of the task's scope.
		//
		//			pointer.setStatus (POINTERSTATUS.TASKUPDATED);
		//
		//			break;
		//
		//		case SCOPE.LOCAL:
		//
		//			// we're a local client. only if the task is local do we trigger the pointer.
		//
		//			if (scope == SCOPE.LOCAL) {
		//				
		//				pointer.setStatus (POINTERSTATUS.TASKUPDATED);
		//
		//			}
		//
		//			break;
		//
		//		default:
		//
		//
		//			break;
		//
		//
		//
		//		}

		//		pointer.setStatus (POINTERSTATUS.TASKUPDATED);





	}

	public TASKSTATUS getStatus ()
	{

		return status;

		//		Int32 value;
		//
		//		if (!taskIntValues.TryGetValue ("status", out value)) {
		//					
		//			Debug.LogError (me + "status value for task not found");
		//
		//			value = (Int32)TASKSTATUS.ACTIVE;
		//		} 
		//
		//		return (TASKSTATUS)value;

	}



	void complete ()
	{

		if (getStatus () != TASKSTATUS.COMPLETE) {

			// make sure a task is only completed once.

			setStatus (TASKSTATUS.COMPLETE);

			//			pointer.taskStatusChanged ();

			//			pointer.setStatus (POINTERSTATUS.TASKUPDATED);


		} else {
			Log.Warning ("A task was completed more than once.",me);

		}

	}

	public void setCallBack (string theCallBackPoint)
	{

		//		Debug.Log ("performing callback: " + theCallBackPoint);

		//		callBackPoint = theCallBackPoint;


		setStringValue ("callBackPoint", theCallBackPoint);



		//		setPointerToUpdated ();





		//		pointer.setStatus (POINTERSTATUS.TASKUPDATED);

		//		#if NETWORKED
		//
		//		taskValuesChangeMask ["callBackPoint"] = true;
		//		hasChanged = true;
		//
		//		#endif

	}

	public void clearCallBack ()
	{

		setStringValue ("callBackPoint", "");


	}


	public string getCallBack ()
	{

		string value;

		if (getStringValue ("callBackPoint", out value)) {

			return value;

		} else {

			return ("");
		}

	}

	public void signOff (String fromMe)
	{

		if (GENERAL.SIGNOFFS == 0) {
			Log.Warning ("Trying to signoff on a task with 0 required signoffs.",me);
		}

		signoffs++;

		//			Debug.Log ("SIGNOFFS "+fromMe + description + " signoffs: " + signoffs + " of " + signoffsRequired);

		if (signoffs == GENERAL.SIGNOFFS) {

			complete ();

		}

	}

}





#if NETWORKED

public class TaskUpdate : MessageBase
{

	public string pointerID, taskID;

	public string description;

	Dictionary<string,Int32> intValues;
	Dictionary<string,float> floatValues;

	public List<string> updatedIntNames;
	public List<Int32> updatedIntValues;

	public List<string> updatedFloatNames;
	public List<float> updatedFloatValues;

	public List<string> updatedQuaternionNames;
	public List<Quaternion> updatedQuaternionValues;

	public List<string> updatedVector3Names;
	public List<Vector3> updatedVector3Values;

	public List<string> updatedStringNames;
	public List<string> updatedStringValues;

	public List<string> updatedUshortNames;
	public List<ushort[]> updatedUshortValues;

	public string debug;

	string me="Taskupdate";

	public override void Deserialize (NetworkReader reader)
	{

		debug = "Deserialised: ";

		// Custom deserialisation.

		//		string packetId = reader.ReadString ();
		//		debug += "/ packetid: " + packetId;

		taskID = reader.ReadString ();
		pointerID = reader.ReadString ();
		description = reader.ReadString ();


		debug += "/ task: " + taskID;
		debug += "/ pointer: " + pointerID;
		debug += "/ description: " + description;


		// Deserialise updated int values.

		updatedIntNames = new List<string> ();
		updatedIntValues = new List<Int32> ();

		int intCount = reader.ReadInt32 ();

		debug += "/ updated ints: " + intCount;

		for (int i = 0; i < intCount; i++) {

			string intName = reader.ReadString ();
			Int32 intValue = reader.ReadInt32 ();

			updatedIntNames.Add (intName);
			updatedIntValues.Add (intValue);

		}

		// Deserialise updated float values.

		updatedFloatNames = new List<string> ();
		updatedFloatValues = new List<float> ();

		int floatCount = reader.ReadInt32 ();

		debug += "/ updated floats: " + floatCount;

		for (int i = 0; i < floatCount; i++) {

			string floatName = reader.ReadString ();
			float floatValue = reader.ReadSingle ();

			updatedFloatNames.Add (floatName);
			updatedFloatValues.Add (floatValue);

		}

		// Deserialise updated quaternion values.

		updatedQuaternionNames = new List<string> ();
		updatedQuaternionValues = new List<Quaternion> ();

		int quaternionCount = reader.ReadInt32 ();

		debug += "/ updated quaternions: " + quaternionCount;

		for (int i = 0; i < quaternionCount; i++) {

			string quaternionName = reader.ReadString ();
			Quaternion quaternionValue = reader.ReadQuaternion ();

			updatedQuaternionNames.Add (quaternionName);
			updatedQuaternionValues.Add (quaternionValue);

		}

		// Deserialise updated vector3 values.

		updatedVector3Names = new List<string> ();
		updatedVector3Values = new List<Vector3> ();

		int vector3Count = reader.ReadInt32 ();

		debug += "/ updated vector3s: " + vector3Count;

		for (int i = 0; i < vector3Count; i++) {

			string vector3Name = reader.ReadString ();
			Vector3 vector3Value = reader.ReadVector3 ();

			updatedVector3Names.Add (vector3Name);
			updatedVector3Values.Add (vector3Value);

		}

		// Deserialise updated string values.

		updatedStringNames = new List<string> ();
		updatedStringValues = new List<string> ();

		int stringCount = reader.ReadInt32 ();

		debug += "/ updated strings: " + stringCount;

		for (int i = 0; i < stringCount; i++) {

			string stringName = reader.ReadString ();
			string stringValue = reader.ReadString ();

			updatedStringNames.Add (stringName);
			updatedStringValues.Add (stringValue);

		}


		// Deserialise updated ushort values.

		updatedUshortNames = new List<string> ();
		updatedUshortValues = new List<ushort[]> ();

		int ushortCount = reader.ReadInt32 ();

		debug += "/ updated ushort arrays: " + ushortCount;

		for (int i = 0; i < ushortCount; i++) {
			
			string ushortName = reader.ReadString ();
			updatedUshortNames.Add (ushortName);

			int ushortArrayLength = reader.ReadInt32 ();

			ushort[] ushortArray = new ushort[ushortArrayLength];

			for (int j = 0; j < ushortArrayLength; j++) {
		
				ushortArray [j] = reader.ReadUInt16 ();
			
			}
					
			updatedUshortValues.Add (ushortArray);

		}

		Log.Message (debug, me, LOGLEVEL.VERBOSE);

		//		Debug.Log (debug);

	}

	public override  void Serialize (NetworkWriter writer)
	{

		debug = "Serialised: ";

		// Custom serialisation.

		//		string packetId = UUID.getId ();
		//		writer.Write (packetId);
		//		debug += "/ packetid: " + packetId;

		// sender ip and storypointer uuid

		writer.Write (taskID);
		writer.Write (pointerID);
		writer.Write (description);

		debug += "/ task: " + taskID;
		debug += "/ pointer: " + pointerID;
		debug += "/ description: " + description;


		// Serialise updated int values.

		writer.Write (updatedIntNames.Count);

		debug += "/ updated ints: " + updatedIntNames.Count;

		for (int i = 0; i < updatedIntNames.Count; i++) {

			writer.Write (updatedIntNames [i]);
			writer.Write (updatedIntValues [i]);

		}

		// Serialise updated float values.

		writer.Write (updatedFloatNames.Count);

		debug += "/ updated floats: " + updatedFloatNames.Count;

		for (int i = 0; i < updatedFloatNames.Count; i++) {

			writer.Write (updatedFloatNames [i]);
			writer.Write (updatedFloatValues [i]);

		}

		// Serialise updated quaternion values.

		writer.Write (updatedQuaternionNames.Count);

		debug += "/ updated quaternions: " + updatedQuaternionNames.Count;

		for (int i = 0; i < updatedQuaternionNames.Count; i++) {

			writer.Write (updatedQuaternionNames [i]);
			writer.Write (updatedQuaternionValues [i]);

		}

		// Serialise updated vector3 values.

		writer.Write (updatedVector3Names.Count);

		debug += "/ updated vector3's: " + updatedVector3Names.Count;

		for (int i = 0; i < updatedVector3Names.Count; i++) {

			writer.Write (updatedVector3Names [i]);
			writer.Write (updatedVector3Values [i]);

		}

		// Serialise updated string values.

		writer.Write (updatedStringNames.Count);

		debug += "/ updated strings: " + updatedStringNames.Count;

		for (int i = 0; i < updatedStringNames.Count; i++) {

			writer.Write (updatedStringNames [i]);
			writer.Write (updatedStringValues [i]);

		}

		// Serialise updated string values.

		writer.Write (updatedUshortNames.Count);

		debug += "/ updated ushorts: " + updatedUshortNames.Count;

		for (int i = 0; i < updatedUshortNames.Count; i++) {

			writer.Write (updatedUshortNames [i]); // name

			writer.Write (updatedUshortValues [i].Length); // length

			for (int j = 0; j < updatedUshortValues [i].Length; j++) {

				writer.Write (updatedUshortValues [i] [j]); // data

			}


		}



		Log.Message (debug, me, LOGLEVEL.VERBOSE);
		//		Debug.Log (debug);

	}

}

#endif





