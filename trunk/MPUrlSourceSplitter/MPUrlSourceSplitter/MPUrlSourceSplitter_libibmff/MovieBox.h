/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#pragma once

#ifndef __MOVIE_BOX_DEFINED
#define __MOVIE_BOX_DEFINED

#include "box.h"

#define MOVIE_BOX_TYPE                                                        L"moov"

class CMovieBox :
  public CBox
{
public:
  // initializes a new instance of CMovieBox class
  CMovieBox(void);

  // destructor
  virtual ~CMovieBox(void);

  /* get methods */

  // gets whole box into buffer
  // @param buffer : the buffer for box data
  // @param length : the length of buffer for data
  // @return : true if all data were successfully stored into buffer, false otherwise
  virtual bool GetBox(uint8_t **buffer, uint32_t *length);

  /* set methods */

  /* other methods */

  // parses data in buffer
  // @param buffer : buffer with box data for parsing
  // @param length : the length of data in buffer
  // @return : true if parsed successfully, false otherwise
  virtual bool Parse(const uint8_t *buffer, uint32_t length);

  // gets box data in human readable format
  // @param indent : string to insert before each line
  // @return : box data in human readable format or NULL if error
  virtual wchar_t *GetParsedHumanReadable(const wchar_t *indent);

protected:

  // gets box size added to size
  // method is called to determine whole box size for storing box into buffer
  // @param size : the size of box calling this method
  // @return : size of box 
  virtual uint64_t GetBoxSize(uint64_t size);
};

#endif