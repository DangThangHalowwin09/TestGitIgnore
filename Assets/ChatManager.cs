using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using TMPro;
using Photon.Pun;

public class ChatManager : MonoBehaviour
{
 private PhotonView _photon;
 public TMP_InputField ChatInput;
 public TextMeshProUGUI ChatContent;
 private List<string> _messages = new List<string>();
 private float _buildDelay = 0f;
 private int _maximumMessages = 5;
 public float timer;

 void Start()
 {
  _photon = GetComponent<PhotonView>();
 }//Start

 [PunRPC]
 void RPC_AddNewMessage(string msg)
 {
  _messages.Add(msg);
   timer = 0;
 }

 public void SendChat(string msg)
 {
  string NewMessage = PhotonNetwork.NickName + ": " + msg;
  _photon.RPC("RPC_AddNewMessage", RpcTarget.All, NewMessage);
 }

 public void SubmitChat()
 {
  string blankCheck = ChatInput.text;
  blankCheck = Regex.Replace(blankCheck, @"\s", "");
  if (blankCheck == "")
  {
   ChatInput.ActivateInputField();
   ChatInput.text = "";
   return;
  }

  SendChat(ChatInput.text);
  ChatInput.ActivateInputField();
  ChatInput.text = "";
 }

 public void BuildChatContents()
 {
  string NewContents = "";
  foreach (string s in _messages)
  {
   NewContents += s + "\n";
  }
  ChatContent.text = NewContents;
 }

 void Update()
 {
  timer += 1*Time.deltaTime;
 
  if (Input.GetKey(KeyCode.KeypadEnter) || Input.GetKey(KeyCode.Return))
  {
   SubmitChat();
  }

  if (PhotonNetwork.InRoom)
  {
   ChatContent.maxVisibleLines = _maximumMessages;
   if (_messages.Count > _maximumMessages)
   {
    _messages.RemoveAt(0);
   }
   if (_buildDelay < Time.time)
   {
    BuildChatContents();
    _buildDelay = Time.time + 0.5f;
   }
  }
  else if (_messages.Count > 0)
  {
   _messages.Clear();
   ChatContent.text = "";
  }
   if (timer > 4)
  {
  if(_messages.Count != 0)
   _messages.RemoveAt(0);
  }
 }//Update



}//End
