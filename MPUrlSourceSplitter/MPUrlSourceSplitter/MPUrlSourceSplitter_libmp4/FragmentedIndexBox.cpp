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

#include "StdAfx.h"

#include "FragmentedIndexBox.h"
#include "BoxCollection.h"

CFragmentedIndexBox::CFragmentedIndexBox(void)
  : CFullBox()
{
  this->type = Duplicate(FRAGMENTED_INDEX_BOX_TYPE);
}

CFragmentedIndexBox::~CFragmentedIndexBox(void)
{
}

/* get methods */

bool CFragmentedIndexBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

/* set methods */

/* other methods */

bool CFragmentedIndexBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CFragmentedIndexBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s"
      ,
      
      previousResult
      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CFragmentedIndexBox::GetBoxSize(void)
{
  return __super::GetBoxSize();
}

bool CFragmentedIndexBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  // in bad case we don't have objects, but still it can be valid box
  bool result = __super::ParseInternal(buffer, length, false);

  if (result)
  {
    if (wcscmp(this->type, FRAGMENTED_INDEX_BOX_TYPE) != 0)
    {
      // incorect box type
      this->parsed = false;
    }
    else
    {
      // box is file type box, parse all values
      uint32_t position = this->HasExtendedHeader() ? FULL_BOX_HEADER_LENGTH_SIZE64 : FULL_BOX_HEADER_LENGTH;
      bool continueParsing = (this->GetSize() <= (uint64_t)length);
      
      if (continueParsing && processAdditionalBoxes)
      {
        this->ProcessAdditionalBoxes(buffer, length, position);
      }

      this->parsed = continueParsing;
    }
  }

  result = this->parsed;

  return result;
}

uint32_t CFragmentedIndexBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}