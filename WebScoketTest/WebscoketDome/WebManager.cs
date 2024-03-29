﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace WebScoketTest.WebscoketDome
{
    public class WebManager
    {
        /// <summary>
        /// 这采用ConcurrentDictionary字典，是线程安全的，不需要加锁
        /// </summary>
        private static ConcurrentDictionary<string, WebSocket> _UserDictionary = new ConcurrentDictionary<string, WebSocket>();

        #region 01-增加用户
        /// <summary>
        /// 增加用户
        /// </summary>
        /// <param name="userKey"></param>
        /// <param name="socket"></param>
        public static bool AddUser(string userKey, WebSocket socket)
        {

            bool flag = _UserDictionary.Select(d => d.Key).ToList().Contains(userKey);
            if (flag == false)
            {
                _UserDictionary[userKey] = socket;
                return true;
            }
            else
            {
                //表示该用户在线
                return false;

            }
        }
        #endregion

        #region 02-移除用户
        /// <summary>
        /// 移除用户
        /// </summary>
        /// <param name="userKey"></param>
        public static void RemoveUser(string userKey)
        {
            WebSocket socket = null;
            _UserDictionary.TryRemove(userKey, out socket);
        }
        #endregion

        #region 03-登录提醒

        /// <summary>
        /// 登录提醒（包括自己）
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static async Task SendLoginSucesssNotice(CancellationToken cancellationToken, string content)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[2048]);
            buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(content));
            //登录提醒（包括自己）
            foreach (var socket in _UserDictionary.Select(d => d.Value))
            {
                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
            }
        }

        #endregion

        #region 04-离开提醒

        /// <summary>
        /// 离开提醒
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static async Task SendOutNotice(CancellationToken cancellationToken, string content)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[2048]);
            buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(content));
            //离开提醒
            foreach (var socket in _UserDictionary.Select(d => d.Value))
            {
                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
            }
        }

        #endregion

        #region 05-群发消息，不包括自己
        /// <summary>
        /// 群发消息，不包括自己
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="content"></param>
        /// <param name="myUserKey">当前用户标记</param>
        /// <returns></returns>
        public static void SendAllMessage(CancellationToken cancellationToken, string content, string myUserKey)
        {
            try
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[2048]);
                buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(content));
                //群发消息，但不包括自己
                foreach (var item in _UserDictionary)
                {
                    if (item.Key.ToString() != myUserKey)
                    {
                        item.Value.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
        #endregion

        #region 06-单发消息
        /// <summary>
        /// 单发消息
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="content"></param>
        /// <param name="receiveKey">接收者的标识</param>
        /// <returns></returns>
        public static void SendSingleMessage(CancellationToken cancellationToken, string content, string receiveKey)
        {
            try
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[2048]);
                buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(content));
                //单发消息
                foreach (var item in _UserDictionary)
                {
                    if (item.Key.ToString() == receiveKey)
                    {
                        item.Value.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
        #endregion

        #region 07-给自己发送消息

        /// <summary>
        /// 给自己发送消息
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="content"></param>
        /// <param name="userKey">当前标记</param>
        /// <returns></returns>
        public static async Task SendToMySelf(CancellationToken cancellationToken, string content, string userKey)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[2048]);
            buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(content));
            //离开提醒
            foreach (var item in _UserDictionary)
            {
                if (item.Key.ToString() == userKey)
                {
                    await item.Value.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
                }
            }
        }
        #endregion
    }
}