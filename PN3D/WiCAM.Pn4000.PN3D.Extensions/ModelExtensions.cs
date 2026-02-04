using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.BendModel.BendTools.Validations;
using WiCAM.Pn4000.Config.DataStructures;

namespace WiCAM.Pn4000.PN3D.Extensions;

public static class ModelExtensions
{
	public static void ColorModel(this Model model, ModelColors3DConfig colors, AnalyzeConfig analyzeConfig, List<ValidationResult> validationResults = null)
	{
		float edgeWidthValidationErr = 5f;
		foreach (FaceHalfEdge edge in model.Shells.SelectMany((Shell x) => x.Faces.SelectMany((Face y) => y.GetAllEdges())))
		{
			if (validationResults != null && validationResults.Any((ValidationResult x) => x.Type == ValidationResult.ResultTypes.Edge && x.Fhe.ID == edge.ID))
			{
				edge.Width = edgeWidthValidationErr;
				edge.Color = colors.ValidationErrorBorderColor.ToBendColor();
			}
			else
			{
				edge.Width = 1f;
				edge.Color = colors.DefaultBorderColor.ToBendColor();
			}
		}
		foreach (FaceGroup item in model.Shells.SelectMany((Shell s) => s.FlatFaceGroups))
		{
			foreach (Face item2 in item.Side0.Concat(item.Side1).Concat(item.SubGroups.SelectMany((FaceGroup sg) => sg.Side0.Concat(sg.Side1))))
			{
				item2.Color = colors.FlatGroupTopBottomFaceColor.ToBendColor();
			}
			foreach (Face item3 in item.ConnectingFaces.Concat(item.SubGroups.SelectMany((FaceGroup sg) => sg.ConnectingFaces)))
			{
				item3.Color = colors.FlatGroupConnectingFaceColor.ToBendColor();
			}
		}
		foreach (FaceGroup group in model.Shells.SelectMany((Shell s) => s.RoundFaceGroups))
		{
			foreach (Face item4 in group.Side0.Concat(group.Side1).Concat(group.SubGroups.SelectMany((FaceGroup sg) => sg.Side0.Concat(sg.Side1))))
			{
				item4.Color = (group.InvalidChangedBendRadius.HasValue ? colors.BendZoneAdjustRadiusErrorColor.ToBendColor() : colors.BendGroupTopBottomFaceColor.ToBendColor());
			}
			foreach (Face item5 in group.ConnectingFaces.Concat(group.SubGroups.SelectMany((FaceGroup sg) => sg.ConnectingFaces)))
			{
				item5.Color = (group.InvalidChangedBendRadius.HasValue ? colors.BendZoneAdjustRadiusErrorColor.ToBendColor() : colors.BendGroupConnectingFaceColor.ToBendColor());
			}
			if (validationResults != null && validationResults.Any((ValidationResult x) => x.Type == ValidationResult.ResultTypes.BendingGroup && x.BendingGroup.ID == group.ID))
			{
				ColorValidationError(group.GetAllFaces());
			}
			foreach (FaceGroup sg2 in group.SubGroups)
			{
				if (validationResults != null && validationResults.Any((ValidationResult x) => x.Type == ValidationResult.ResultTypes.BendingGroup && x.BendingGroup.ID == sg2.ID))
				{
					ColorValidationError(sg2.GetAllFaces());
				}
			}
		}
		foreach (Macro macro in model.Shells.SelectMany((Shell s) => s.Macros))
		{
			Color color = new Color(1f, 1f, 1f, 1f);
			if (!(macro is SimpleHole))
			{
				if (!(macro is Border))
				{
					if (!(macro is TwoSidedCounterSink))
					{
						if (!(macro is EmbossedCounterSink))
						{
							if (!(macro is PressNut))
							{
								if (!(macro is Louver))
								{
									if (!(macro is BridgeLance))
									{
										if (!(macro is Chamfer))
										{
											if (!(macro is StepDrilling))
											{
												if (!(macro is CounterSink))
												{
													if (!(macro is Lance))
													{
														if (!(macro is EmbossmentStamp))
														{
															Deepening deepening = macro as Deepening;
															if (deepening == null)
															{
																if (!(macro is BlindHole))
																{
																	if (!(macro is ConicBlindHole))
																	{
																		if (!(macro is SphericalBlindHole))
																		{
																			if (!(macro is EmbossedCircle))
																			{
																				if (!(macro is EmbossedLine))
																				{
																					if (!(macro is EmbossedSquare))
																					{
																						if (!(macro is EmbossedRectangle))
																						{
																							if (!(macro is EmbossedSquareRounded))
																							{
																								if (!(macro is EmbossedRectangleRounded))
																								{
																									if (!(macro is EmbossedFreeform))
																									{
																										if (macro is ManufacturingMacro)
																										{
																											color = colors.ManufacturingMacroFaceColor.ToBendColor();
																										}
																									}
																									else
																									{
																										color = colors.MacroEmbossedFreeformFaceColor.ToBendColor();
																									}
																								}
																								else
																								{
																									color = colors.MacroEmbossedRectangleRoundedFaceColor.ToBendColor();
																								}
																							}
																							else
																							{
																								color = colors.MacroEmbossedSquareRoundedFaceColor.ToBendColor();
																							}
																						}
																						else
																						{
																							color = colors.MacroEmbossedRectangleFaceColor.ToBendColor();
																						}
																					}
																					else
																					{
																						color = colors.MacroEmbossedSquareFaceColor.ToBendColor();
																					}
																				}
																				else
																				{
																					color = colors.MacroEmbossedLineFaceColor.ToBendColor();
																				}
																			}
																			else
																			{
																				color = colors.MacroEmbossedCircleFaceColor.ToBendColor();
																			}
																		}
																		else
																		{
																			color = colors.MacroSphericalBlindHoleFaceColor.ToBendColor();
																		}
																	}
																	else
																	{
																		color = colors.MacroConicBlindHoleFaceColor.ToBendColor();
																	}
																}
																else
																{
																	color = colors.MacroBlindHoleFaceColor.ToBendColor();
																}
															}
															else
															{
																List<double>? deepeningListDepth = analyzeConfig.DeepeningListDepth;
																if (deepeningListDepth == null || deepeningListDepth.Count == 0 || analyzeConfig.DeepeningListDepth.All((double d) => d <= deepening.Depth) || analyzeConfig.ShowDeepeningLevels == false)
																{
																	color = colors.MacroDeepeningFaceColor.ToBendColor();
																}
																else
																{
																	int index = analyzeConfig.DeepeningListDepth.FindIndex((double d) => d > deepening.Depth);
																	color = analyzeConfig.DeepeningListColor3D[index].ToBendColor();
																}
															}
														}
														else
														{
															color = colors.MacroEmbossmentStampFaceColor.ToBendColor();
														}
													}
													else
													{
														color = colors.MacroLanceFaceColor.ToBendColor();
													}
												}
												else
												{
													color = colors.MacroCounterSinkFaceColor.ToBendColor();
												}
											}
											else
											{
												color = colors.MacroStepDrillingFaceColor.ToBendColor();
											}
										}
										else
										{
											color = colors.MacroChamferFaceColor.ToBendColor();
										}
									}
									else
									{
										color = colors.MacroBridgeFaceColor.ToBendColor();
									}
								}
								else
								{
									color = colors.MacroLouverFaceColor.ToBendColor();
								}
							}
							else
							{
								color = colors.MacroBoltFaceColor.ToBendColor();
							}
						}
						else
						{
							color = colors.MacroEmbossedCounterSinkFaceColor.ToBendColor();
						}
					}
					else
					{
						color = colors.MacroTwoSidedCounterSinkFaceColor.ToBendColor();
					}
				}
				else
				{
					color = colors.MacroBorderColor.ToBendColor();
				}
			}
			else
			{
				color = colors.MacroSimpleHoleFaceColor.ToBendColor();
			}
			if (validationResults != null && validationResults.Any((ValidationResult x) => x.Type == ValidationResult.ResultTypes.Macro && x.Macro.ID == macro.ID))
			{
				ColorValidationError(macro.Faces);
			}
			if (macro == null)
			{
				continue;
			}
			foreach (Face face in macro.Faces)
			{
				face.Color = color;
			}
		}
		foreach (Model subModel in model.SubModels)
		{
			subModel.ColorModel(colors, analyzeConfig, validationResults);
		}
		foreach (Face item6 in model.PartInfo.NotConformFaces.SelectMany((KeyValuePair<FaceGroup, List<Face>> kvp) => kvp.Value))
		{
			item6.Color = colors.NotBendableFaceColor.ToBendColor();
		}
		foreach (Face item7 in from f in model.Shells.SelectMany((Shell shell) => shell.Faces)
			where f.IsTessellated.HasValue
			select f)
		{
			item7.Color = colors.CutOutColor.ToBendColor();
		}
		void ColorValidationError(IEnumerable<Face> faces)
		{
			foreach (FaceHalfEdge item8 in faces.SelectMany((Face x) => x.GetAllEdges()))
			{
				item8.Color = colors.ValidationErrorBorderColor.ToBendColor();
				item8.Width = edgeWidthValidationErr;
			}
		}
	}

	public static void HighlightColorModel(this Model model, ModelColors3DConfig colors, HashSet<FaceHalfEdge> primaryEdges, HashSet<FaceHalfEdge> secondaryEdges = null)
	{
		Color value = colors.SelectedObjectHighlightBorderColorPrimary.ToBendColor();
		Color value2 = colors.SelectedObjectHighlightBorderColorSecondary.ToBendColor();
		Stack<Model> stack = new Stack<Model>();
		stack.Push(model);
		while (stack.Count > 0)
		{
			Model model2 = stack.Pop();
			foreach (Model subModel in model2.SubModels)
			{
				stack.Push(subModel);
			}
			foreach (Face item in model2.Shells.SelectMany((Shell shell) => shell.Faces))
			{
				item.HighlightColor = null;
				foreach (FaceHalfEdge allEdge in item.GetAllEdges())
				{
					if (primaryEdges != null && primaryEdges.Contains(allEdge))
					{
						allEdge.HighlightWidth = 5f;
						allEdge.HighlightColor = value;
					}
					else if (secondaryEdges != null && secondaryEdges.Contains(allEdge))
					{
						allEdge.HighlightWidth = 5f;
						allEdge.HighlightColor = value2;
					}
					else
					{
						allEdge.HighlightWidth = null;
						allEdge.HighlightColor = null;
					}
				}
			}
		}
	}
}
