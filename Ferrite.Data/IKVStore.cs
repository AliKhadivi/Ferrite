﻿/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;

namespace Ferrite.Data;

public interface IKVStore
{
	void Init(string path);
	void Put(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value);
	byte[] Get(ReadOnlySpan<byte> key);
	void Remove(ReadOnlySpan<byte> key);
}


