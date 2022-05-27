#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Tekla.Structures.InpParser
{
	public class Parser
	{
		private readonly Dictionary<TSModelObjectTypes, TSModelObject> definedModelObjects;

		private Dictionary<string, TSTabPageDefinition> definedTabPages;

		private int lineNumber = 1;

		private bool overrideExisting;

		private bool validationOn = true;

		public Dictionary<TSModelObjectTypes, TSModelObject> DefinedModelObjects => definedModelObjects;

		public Dictionary<string, TSTabPageDefinition> DefinedTabPages => definedTabPages;

		public bool ValidationOn
		{
			get
			{
				return validationOn;
			}
			set
			{
				validationOn = value;
			}
		}

		public Parser()
		{
			definedModelObjects = new Dictionary<TSModelObjectTypes, TSModelObject>();
			definedTabPages = new Dictionary<string, TSTabPageDefinition>();
		}

		public List<UDA> FindUdas(UDATypes udaType)
		{
			List<string> list = new List<string>();
			List<UDA> list2 = new List<UDA>();
			foreach (TSTabPageDefinition value in DefinedTabPages.Values)
			{
				foreach (TSTabPageObject @object in value.Objects)
				{
					if (@object is UDA)
					{
						UDA uDA = @object as UDA;
						if (uDA.ValueType == udaType && !list.Contains(uDA.Name))
						{
							list.Add(uDA.Name);
							list2.Add(uDA);
						}
					}
				}
			}
			foreach (TSModelObject value2 in DefinedModelObjects.Values)
			{
				if (value2.TabPages == null || value2.TabPages.Count != 1 || value2.TabPages[0].Definition == null)
				{
					continue;
				}
				foreach (TSTabPageObject object2 in value2.TabPages[0].Definition.Objects)
				{
					if (object2 is UDA)
					{
						UDA uDA2 = object2 as UDA;
						if (uDA2.ValueType == udaType && !list.Contains(uDA2.Name))
						{
							list.Add(uDA2.Name);
							list2.Add(uDA2);
						}
					}
				}
			}
			return list2;
		}

		public void Parse(string filePath, bool overrideExisting)
		{
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException();
			}
			this.overrideExisting = overrideExisting;
			lineNumber = 1;
			using FileStream inputStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			try
			{
				while (true)
				{
					TSModelObject tSModelObject = TryGetModelObject(inputStream);
					if (tSModelObject != null)
					{
						if (!definedModelObjects.ContainsKey(tSModelObject.Type))
						{
							definedModelObjects.Add(tSModelObject.Type, tSModelObject);
						}
						else if (this.overrideExisting)
						{
							definedModelObjects.Remove(tSModelObject.Type);
							definedModelObjects.Add(tSModelObject.Type, tSModelObject);
						}
					}
				}
			}
			catch (EOFException ex)
			{
				if (!ex.IsCorrectEnd)
				{
					throw ex;
				}
			}
			catch (WrongFormatException ex2)
			{
				Trace.WriteLine("Line " + ex2.LineNumber + ": " + ex2.ToString());
			}
		}

		private UDAValue TryGetAttributeValue(FileStream inputStream)
		{
			long position = inputStream.Position;
			int num = lineNumber;
			if (!TryGetKeWordY(inputStream, KeyWords.value.ToString()))
			{
				inputStream.Seek(position, SeekOrigin.Begin);
				lineNumber = num;
				return null;
			}
			TryGetPunctuation(inputStream, "(");
			Token lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("Attribute value string parameter", TokenType.String));
			}
			if (lexeme.TokenType != TokenType.String)
			{
				throw new WrongFormatException(lexeme, new Token(string.Empty, TokenType.String), lineNumber);
			}
			UDAValue uDAValue = new UDAValue(lexeme.Value);
			TryGetPunctuation(inputStream, ",");
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("0-2", TokenType.Number));
			}
			if (lexeme.TokenType != TokenType.Number)
			{
				throw new WrongFormatException(lexeme, new Token("0-2", TokenType.Number), lineNumber);
			}
			if (!int.TryParse(lexeme.Value, out var result))
			{
				throw new WrongFormatException(lexeme, new Token("0-2", TokenType.Number), lineNumber);
			}
			uDAValue.DefaultSwitch = result;
			TryGetPunctuation(inputStream, ")");
			return uDAValue;
		}

		private bool TryGetKeWordY(FileStream inputStream, string keyWord)
		{
			Token lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null || lexeme.TokenType != TokenType.Identifier || !(lexeme.Value == keyWord))
			{
				return false;
			}
			return true;
		}

		private TSModelObject TryGetModelObject(FileStream inputStream)
		{
			long position = inputStream.Position;
			int num = lineNumber;
			Token lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(correctEnd: true);
			}
			if (lexeme.TokenType != TokenType.Identifier)
			{
				throw new WrongFormatException(lexeme, new Token("Model object identifier", TokenType.Identifier), lineNumber);
			}
			TSModelObject tSModelObject;
			try
			{
				tSModelObject = new TSModelObject((TSModelObjectTypes)Enum.Parse(typeof(TSModelObjectTypes), lexeme.Value, ignoreCase: true));
			}
			catch (ArgumentException)
			{
				throw new WrongFormatException(lexeme, new Token("Correct model object identifier", TokenType.Identifier), lineNumber);
			}
			TryGetPunctuation(inputStream, "(");
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("Dummy number", TokenType.Number));
			}
			if (lexeme.TokenType != TokenType.Number)
			{
				throw new WrongFormatException(lexeme, new Token("Dummy number", TokenType.Number), lineNumber);
			}
			if (!int.TryParse(lexeme.Value, out var result))
			{
				throw new WrongFormatException(lexeme, new Token("Dummy number as integer", TokenType.Number), lineNumber);
			}
			tSModelObject.DummyNumber = result;
			TryGetPunctuation(inputStream, ",");
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("Model object name", TokenType.String));
			}
			if (lexeme.TokenType != TokenType.String)
			{
				throw new WrongFormatException(lexeme, new Token("Model object name", TokenType.String), lineNumber);
			}
			tSModelObject.Name = lexeme.Value;
			TryGetPunctuation(inputStream, ")");
			TryGetPunctuation(inputStream, "{");
			bool flag = false;
			while (!flag)
			{
				TSTabPageDefinition tSTabPageDefinition = TryGetTabPageDefinition(inputStream);
				if (tSTabPageDefinition == null)
				{
					TSTabPageDeclaration tSTabPageDeclaration = TryGetTabPageDeclaration(inputStream);
					if (tSTabPageDeclaration == null)
					{
						UDA uDA;
						try
						{
							uDA = TryGetUDA(inputStream);
						}
						catch (WrongFormatException ex2)
						{
							Trace.WriteLine("Line " + ex2.LineNumber + ": " + ex2.ToString());
							while (lexeme.TokenType != 0 || !(lexeme.Value == "}"))
							{
								lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
							}
							continue;
						}
						if (uDA == null)
						{
							flag = true;
							continue;
						}
						if (tSModelObject.Attributes == null)
						{
							tSModelObject.Attributes = new List<UDA>();
						}
						tSModelObject.Attributes.Add(uDA);
					}
					else
					{
						if (tSModelObject.TabPages == null)
						{
							tSModelObject.TabPages = new List<TSTabPageDeclaration>();
						}
						if (tSTabPageDeclaration.Definition == null && !definedTabPages.ContainsKey(tSTabPageDeclaration.Name))
						{
							throw new WrongFormatException(new Token("Not defined yet TabPage name", TokenType.String));
						}
						tSModelObject.TabPages.Add(tSTabPageDeclaration);
					}
				}
				else
				{
					if (definedTabPages == null)
					{
						definedTabPages = new Dictionary<string, TSTabPageDefinition>();
					}
					if (!definedTabPages.ContainsKey(tSTabPageDefinition.Name))
					{
						definedTabPages.Add(tSTabPageDefinition.Name, tSTabPageDefinition);
					}
					else if (overrideExisting)
					{
						definedTabPages.Remove(tSTabPageDefinition.Name);
						definedTabPages.Add(tSTabPageDefinition.Name, tSTabPageDefinition);
					}
				}
			}
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token(KeyWords.modify.ToString(), TokenType.Identifier));
			}
			if (lexeme.TokenType != TokenType.Identifier || !(lexeme.Value == KeyWords.modify.ToString()))
			{
				if (lexeme.TokenType != 0 || !(lexeme.Value == "}"))
				{
					throw new WrongFormatException(null, new Token("modify", TokenType.Identifier), lineNumber);
				}
				return tSModelObject;
			}
			TryGetPunctuation(inputStream, "(");
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("0-1", TokenType.Number));
			}
			if (lexeme.TokenType != TokenType.Number)
			{
				throw new WrongFormatException();
			}
			if (!int.TryParse(lexeme.Value, out result))
			{
				throw new WrongFormatException(lexeme, new Token("0-1", TokenType.Number), lineNumber);
			}
			switch (result)
			{
			case 0:
				tSModelObject.Modify = false;
				break;
			case 1:
				tSModelObject.Modify = true;
				break;
			default:
				throw new WrongFormatException(lexeme, new Token("0-1", TokenType.Number), lineNumber);
			}
			TryGetPunctuation(inputStream, ")");
			TryGetPunctuation(inputStream, "}");
			return tSModelObject;
		}

		private Picture TryGetPicture(FileStream inputStream)
		{
			long position = inputStream.Position;
			int num = lineNumber;
			if (!TryGetKeWordY(inputStream, KeyWords.picture.ToString()))
			{
				inputStream.Seek(position, SeekOrigin.Begin);
				lineNumber = num;
				return null;
			}
			TryGetPunctuation(inputStream, "(");
			Token lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("Picture name", TokenType.String));
			}
			if (lexeme.TokenType != TokenType.String)
			{
				throw new WrongFormatException(lexeme, new Token("Picture name", TokenType.String), lineNumber);
			}
			Picture picture = new Picture(lexeme.Value);
			TryGetPunctuation(inputStream, ",");
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("Picture width", TokenType.Number));
			}
			if (lexeme.TokenType != TokenType.Number)
			{
				throw new WrongFormatException(lexeme, new Token("Picture width", TokenType.Number), lineNumber);
			}
			if (!int.TryParse(lexeme.Value, out var result))
			{
				throw new WrongFormatException(lexeme, new Token("Picture width as integer number", TokenType.Number), lineNumber);
			}
			picture.Width = result;
			TryGetPunctuation(inputStream, ",");
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("Picture height", TokenType.Number));
			}
			if (lexeme.TokenType != TokenType.Number)
			{
				throw new WrongFormatException(lexeme, new Token("Picture height", TokenType.Number), lineNumber);
			}
			if (!int.TryParse(lexeme.Value, out result))
			{
				throw new WrongFormatException(lexeme, new Token("Picture height as integer number", TokenType.Number), lineNumber);
			}
			picture.Height = result;
			TryGetPunctuation(inputStream, ",");
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("Picture X coordinate", TokenType.Number));
			}
			if (lexeme.TokenType != TokenType.Number)
			{
				throw new WrongFormatException(lexeme, new Token("Picture X coordinate", TokenType.Number), lineNumber);
			}
			if (!int.TryParse(lexeme.Value, out result))
			{
				throw new WrongFormatException(lexeme, new Token("Picture X coordinate as integer number", TokenType.Number), lineNumber);
			}
			picture.X = result;
			TryGetPunctuation(inputStream, ",");
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("Picture Y coordinate", TokenType.Number));
			}
			if (lexeme.TokenType != TokenType.Number)
			{
				throw new WrongFormatException(lexeme, new Token("Picture Y coordinate", TokenType.Number), lineNumber);
			}
			if (!int.TryParse(lexeme.Value, out result))
			{
				throw new WrongFormatException(lexeme, new Token("Picture Y coordinate as integer number", TokenType.Number), lineNumber);
			}
			picture.Y = result;
			TryGetPunctuation(inputStream, ")");
			return picture;
		}

		private void TryGetPunctuation(FileStream inputStream, string symbol)
		{
			Token lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token(symbol, TokenType.Punctuation));
			}
			if (lexeme.TokenType != 0 || !(lexeme.Value == symbol))
			{
				throw new WrongFormatException(lexeme, new Token(symbol, TokenType.Punctuation), lineNumber);
			}
		}

		private TSTabPageDeclaration TryGetTabPageDeclaration(FileStream inputStream)
		{
			long position = inputStream.Position;
			int num = lineNumber;
			if (!TryGetKeWordY(inputStream, KeyWords.tab_page.ToString()))
			{
				inputStream.Seek(position, SeekOrigin.Begin);
				lineNumber = num;
				return null;
			}
			TryGetPunctuation(inputStream, "(");
			Token lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("TabPage name", TokenType.String));
			}
			if (lexeme.TokenType != TokenType.String)
			{
				throw new WrongFormatException(lexeme, new Token("TabPage name", TokenType.String), lineNumber);
			}
			TSTabPageDeclaration tSTabPageDeclaration = new TSTabPageDeclaration(lexeme.Value);
			TryGetPunctuation(inputStream, ",");
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("TabPage title", TokenType.String));
			}
			if (lexeme.TokenType != TokenType.String)
			{
				throw new WrongFormatException(lexeme, new Token("TabPage title", TokenType.String), lineNumber);
			}
			tSTabPageDeclaration.Prompt = lexeme.Value;
			TryGetPunctuation(inputStream, ",");
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("TabPage index", TokenType.Number));
			}
			if (lexeme.TokenType != TokenType.Number)
			{
				throw new WrongFormatException(lexeme, new Token("TabPage index", TokenType.Number), lineNumber);
			}
			if (!int.TryParse(lexeme.Value, out var result))
			{
				throw new WrongFormatException(lexeme, new Token("TabPage index as integer number", TokenType.Number), lineNumber);
			}
			tSTabPageDeclaration.Index = result;
			TryGetPunctuation(inputStream, ")");
			if (tSTabPageDeclaration.Name != string.Empty)
			{
				return tSTabPageDeclaration;
			}
			TSTabPageDefinition definition = new TSTabPageDefinition(string.Empty);
			TryGetTabPageDefinitionBody(inputStream, ref definition);
			tSTabPageDeclaration.Definition = definition;
			return tSTabPageDeclaration;
		}

		private TSTabPageDefinition TryGetTabPageDefinition(FileStream inputStream)
		{
			long position = inputStream.Position;
			int num = lineNumber;
			if (!TryGetKeWordY(inputStream, KeyWords.tab_page.ToString()))
			{
				inputStream.Seek(position, SeekOrigin.Begin);
				lineNumber = num;
				return null;
			}
			TryGetPunctuation(inputStream, "(");
			Token lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("TabPage name", TokenType.String));
			}
			if (lexeme.TokenType != TokenType.String)
			{
				throw new WrongFormatException(lexeme, new Token("TabPage name", TokenType.String), lineNumber);
			}
			TSTabPageDefinition definition = new TSTabPageDefinition(lexeme.Value);
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token(")", TokenType.Punctuation));
			}
			if (lexeme.TokenType != 0 || !(lexeme.Value == ")"))
			{
				if (lexeme.TokenType != 0 || !(lexeme.Value == ","))
				{
					throw new WrongFormatException(lexeme, new Token(")", TokenType.Punctuation), lineNumber);
				}
				inputStream.Seek(position, SeekOrigin.Begin);
				lineNumber = num;
				return null;
			}
			TryGetTabPageDefinitionBody(inputStream, ref definition);
			return definition;
		}

		private void TryGetTabPageDefinitionBody(FileStream inputStream, ref TSTabPageDefinition definition)
		{
			TryGetPunctuation(inputStream, "{");
			bool flag = false;
			while (!flag)
			{
				UDA uDA;
				try
				{
					uDA = TryGetUDA(inputStream);
				}
				catch (WrongFormatException ex)
				{
					Trace.WriteLine("Line " + ex.LineNumber + ": " + ex.ToString());
					Token lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
					while (lexeme.TokenType != 0 || !(lexeme.Value == "}"))
					{
						lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
					}
					continue;
				}
				if (uDA == null)
				{
					Picture picture = TryGetPicture(inputStream);
					if (picture == null)
					{
						flag = true;
						continue;
					}
					if (definition.Objects == null)
					{
						definition.Objects = new List<TSTabPageObject>();
					}
					definition.Objects.Add(picture);
				}
				else
				{
					if (definition.Objects == null)
					{
						definition.Objects = new List<TSTabPageObject>();
					}
					definition.Objects.Add(uDA);
				}
			}
			TryGetPunctuation(inputStream, "}");
		}

		private UDA TryGetUDA(FileStream inputStream)
		{
			long position = inputStream.Position;
			int num = lineNumber;
			Token lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null || lexeme.TokenType != TokenType.Identifier || (!(lexeme.Value == KeyWords.attribute.ToString()) && !(lexeme.Value == KeyWords.unique_attribute.ToString())))
			{
				inputStream.Seek(position, SeekOrigin.Begin);
				lineNumber = num;
				return null;
			}
			UDA uDA = ((lexeme.Value == KeyWords.unique_attribute.ToString()) ? new UDA(isUnique: true) : new UDA(isUnique: false));
			TryGetPunctuation(inputStream, "(");
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("Attribute name", TokenType.String));
			}
			if (lexeme.TokenType != TokenType.String)
			{
				throw new WrongFormatException(lexeme, new Token("Attribute name", TokenType.String), lineNumber);
			}
			uDA.Name = lexeme.Value;
			TryGetPunctuation(inputStream, ",");
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("Label text", TokenType.String));
			}
			if (lexeme.TokenType != TokenType.String)
			{
				throw new WrongFormatException(lexeme, new Token("Label text", TokenType.String), lineNumber);
			}
			uDA.LabelText = lexeme.Value;
			TryGetPunctuation(inputStream, ",");
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("Type of attribute value", TokenType.Identifier));
			}
			if (lexeme.TokenType != TokenType.Identifier)
			{
				throw new WrongFormatException(lexeme, new Token("Type of attribute value", TokenType.Identifier), lineNumber);
			}
			try
			{
				uDA.ValueType = (UDATypes)Enum.Parse(typeof(UDATypes), lexeme.Value, ignoreCase: true);
			}
			catch (ArgumentException)
			{
				throw new WrongFormatException(lexeme, new Token("Correct type identifier", TokenType.Identifier), lineNumber);
			}
			TryGetPunctuation(inputStream, ",");
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("Format string", TokenType.String));
			}
			if (lexeme.TokenType != TokenType.String)
			{
				throw new WrongFormatException(lexeme, new Token("Format string", TokenType.String), lineNumber);
			}
			if (!lexeme.Value.StartsWith("%"))
			{
				throw new WrongFormatException(lexeme, new Token("Correct format string", TokenType.String), lineNumber);
			}
			uDA.FieldFormat = lexeme.Value;
			TryGetPunctuation(inputStream, ",");
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("yes|no", TokenType.Identifier));
			}
			if (validationOn && (lexeme.TokenType != TokenType.Identifier || (!(lexeme.Value == "yes") && !(lexeme.Value == "no"))))
			{
				throw new WrongFormatException(lexeme, new Token("yes|no", TokenType.Identifier), lineNumber);
			}
			uDA.SpecialFlag = lexeme.Value == "yes";
			TryGetPunctuation(inputStream, ",");
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("Check switch identifier", TokenType.Identifier));
			}
			if (lexeme.TokenType != TokenType.Identifier)
			{
				throw new WrongFormatException(lexeme, new Token("Check switch identifier", TokenType.Identifier), lineNumber);
			}
			if (!Enum.IsDefined(typeof(CheckSwitchValues), lexeme.Value))
			{
				throw new WrongFormatException(lexeme, new Token("Correct check switch identifier", TokenType.Identifier), lineNumber);
			}
			uDA.CheckSwitch = (CheckSwitchValues)Enum.Parse(typeof(CheckSwitchValues), lexeme.Value);
			TryGetPunctuation(inputStream, ",");
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("Attribute value max", TokenType.String));
			}
			if (lexeme.TokenType != TokenType.String)
			{
				throw new WrongFormatException(lexeme, new Token("Attribute value max", TokenType.String), lineNumber);
			}
			if (validationOn && !char.IsNumber(lexeme.Value, 0))
			{
				throw new WrongFormatException(lexeme, new Token("Attribute value max number as string", TokenType.String), lineNumber);
			}
			uDA.AttributeValueMax = lexeme.Value;
			TryGetPunctuation(inputStream, ",");
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token("Attribute value min", TokenType.String));
			}
			if (lexeme.TokenType != TokenType.String)
			{
				throw new WrongFormatException(lexeme, new Token("Attribute value max", TokenType.String), lineNumber);
			}
			if (validationOn && !char.IsNumber(lexeme.Value, 0))
			{
				throw new WrongFormatException(lexeme, new Token("Attribute value max number as string", TokenType.String), lineNumber);
			}
			uDA.AttributeValueMin = lexeme.Value;
			lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
			if (lexeme == null)
			{
				throw new EOFException(new Token(")", TokenType.Punctuation));
			}
			if (lexeme.TokenType != 0 || !(lexeme.Value == ")"))
			{
				if (lexeme.TokenType != 0 || !(lexeme.Value == ","))
				{
					throw new WrongFormatException(lexeme, new Token(")|,", TokenType.Punctuation), lineNumber);
				}
				lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
				if (lexeme == null)
				{
					throw new EOFException(new Token("Integer number", TokenType.Number));
				}
				if (lexeme.TokenType == TokenType.String)
				{
					uDA.ToggleField = lexeme.Value;
					lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
					if (lexeme == null)
					{
						throw new EOFException(new Token(")", TokenType.Punctuation));
					}
				}
				else
				{
					if (lexeme.TokenType != TokenType.Number)
					{
						throw new WrongFormatException(lexeme, new Token("Integer number", TokenType.Number), lineNumber);
					}
					if (!int.TryParse(lexeme.Value, out var result))
					{
						throw new WrongFormatException(lexeme, new Token("Integer number", TokenType.Number), lineNumber);
					}
					uDA.PositionValue1 = result;
					lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
					if (lexeme == null)
					{
						throw new EOFException(new Token(")|,", TokenType.Punctuation));
					}
					if (lexeme.TokenType != 0 || !(lexeme.Value == ")"))
					{
						if (lexeme.TokenType != 0 || !(lexeme.Value == ","))
						{
							throw new WrongFormatException(lexeme, new Token(")|,", TokenType.Punctuation), lineNumber);
						}
						lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
						if (lexeme == null)
						{
							throw new EOFException(new Token("Integer number", TokenType.Number));
						}
						if (lexeme.TokenType == TokenType.String)
						{
							uDA.ToggleField = lexeme.Value;
							lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
							if (lexeme == null)
							{
								throw new EOFException(new Token(")", TokenType.Punctuation));
							}
						}
						else
						{
							if (lexeme.TokenType != TokenType.Number)
							{
								throw new WrongFormatException(lexeme, new Token("Integer number", TokenType.Number), lineNumber);
							}
							if (!int.TryParse(lexeme.Value, out result))
							{
								throw new WrongFormatException(lexeme, new Token("Integer number", TokenType.Number), lineNumber);
							}
							uDA.PositionValue2 = result;
							lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
							if (lexeme == null)
							{
								throw new EOFException(new Token(")|,", TokenType.Punctuation));
							}
							if (lexeme.TokenType != 0 || !(lexeme.Value == ")"))
							{
								if (lexeme.TokenType != 0 || !(lexeme.Value == ","))
								{
									throw new WrongFormatException(new Token(")|,", TokenType.Punctuation));
								}
								lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
								if (lexeme == null)
								{
									throw new EOFException(new Token("Integer number", TokenType.Number));
								}
								if (lexeme.TokenType == TokenType.String)
								{
									uDA.ToggleField = lexeme.Value;
									lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
									if (lexeme == null)
									{
										throw new EOFException(new Token(")", TokenType.Punctuation));
									}
								}
								else
								{
									if (lexeme.TokenType != TokenType.Number)
									{
										throw new WrongFormatException(lexeme, new Token("Integer number", TokenType.Number), lineNumber);
									}
									if (!int.TryParse(lexeme.Value, out result))
									{
										throw new WrongFormatException(lexeme, new Token("Integer number", TokenType.Number), lineNumber);
									}
									uDA.PositionValue3 = result;
									lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
									if (lexeme == null)
									{
										throw new EOFException(new Token(")", TokenType.Punctuation));
									}
									if (lexeme.TokenType != 0 || !(lexeme.Value == ")"))
									{
										if (lexeme.TokenType != 0 || !(lexeme.Value == ","))
										{
											throw new WrongFormatException(lexeme, new Token(")|,", TokenType.Punctuation), lineNumber);
										}
										lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
										if (lexeme == null)
										{
											throw new EOFException(new Token("Toggle field", TokenType.String));
										}
										if (lexeme.TokenType != TokenType.String)
										{
											throw new WrongFormatException(lexeme, new Token("Toggle field", TokenType.String), lineNumber);
										}
										uDA.ToggleField = lexeme.Value;
										lexeme = Lexer.GetLexeme(inputStream, ref lineNumber);
										if (lexeme == null)
										{
											throw new EOFException(new Token(")", TokenType.Punctuation));
										}
									}
								}
							}
						}
					}
				}
			}
			if (lexeme.TokenType != 0 || !(lexeme.Value == ")"))
			{
				throw new WrongFormatException(lexeme, new Token(")", TokenType.Punctuation), lineNumber);
			}
			if (uDA.ValueType == UDATypes.Label || uDA.ValueType == UDATypes.label2 || uDA.ValueType == UDATypes.label3)
			{
				return uDA;
			}
			if (uDA.ValueType == UDATypes.stud_length || uDA.ValueType == UDATypes.stud_size || uDA.ValueType == UDATypes.stud_standard || uDA.ValueType == UDATypes.bolt_type || uDA.ValueType == UDATypes.Bolt_standard || uDA.ValueType == UDATypes.Bolt_standard)
			{
				return uDA;
			}
			TryGetPunctuation(inputStream, "{");
			UDAValue uDAValue = TryGetAttributeValue(inputStream);
			if (uDAValue == null)
			{
				throw new WrongFormatException(new Token("At least one attribute definition", TokenType.Identifier));
			}
			uDA.Values = new List<UDAValue>();
			do
			{
				uDA.Values.Add(uDAValue);
				uDAValue = TryGetAttributeValue(inputStream);
			}
			while (uDAValue != null);
			TryGetPunctuation(inputStream, "}");
			return uDA;
		}
	}
}
