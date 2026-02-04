using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.BendModel.BendTools.Validations;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendModel.Serialization.SerializationStructure.ManufacturingData;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.Converter;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Enums;
using WiCAM.Pn4000.PN3D.Extensions;
using WiCAM.Pn4000.PN3D.Popup.Information;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.TabUnfold;

internal class InfoViewModel : SubViewModelBase, IInfoViewModel, ISubViewModel
{
	private readonly IScreen3DMain _screen3DMain;

	private readonly ITranslator _translator;

	private readonly IDoc3d _currentDoc;

	private readonly Color _highlightColor;

	private bool _isActive;

	private readonly List<(FaceHalfEdge, Model)> _highlightedEdges = new List<(FaceHalfEdge, Model)>();

	private ModelViewMode _currentViewMode;

	private IScreen3D Screen3D => _screen3DMain.Screen3D;

	public InfoViewModel(ITranslator translator, IScreen3DMain screen3DMain, IDoc3d currentDoc, IConfigProvider configProvider)
	{
		_translator = translator;
		_screen3DMain = screen3DMain;
		Color highlightColor = configProvider.InjectOrCreate<ModelColors3DConfig>().SelectedObjectHighlightBorderColorPrimary.ToBendColor();
		_highlightColor = highlightColor;
		_currentDoc = currentDoc;
	}

	public void SetActiveModelType(ModelViewMode newMode)
	{
		_currentViewMode = newMode;
	}

	public override bool Close()
	{
		if (_isActive)
		{
			Screen3D.HideScreenInfoText();
		}
		return base.Close();
	}

	public override void SetActive(bool active)
	{
		base.SetActive(active);
		if (active != _isActive)
		{
			_isActive = active;
			if (!_isActive)
			{
				Screen3D.HideScreenInfoText();
			}
		}
	}

	public override void MouseSelectTriangle(object sender, ITriangleEventArgs e)
	{
		base.MouseSelectTriangle(sender, e);
		if (e.MouseEventArgs.ChangedButton == MouseButton.Left)
		{
			MouseSelectTriangleInfo(sender, e.Tri, e.Model);
			e.MouseEventArgs.Handled = true;
		}
	}

	public override void ColorModelParts(IPaintTool paintTool)
	{
		base.ColorModelParts(paintTool);
		if (!_isActive)
		{
			return;
		}
		foreach (var (edge, model) in _highlightedEdges)
		{
			paintTool.SetEdgeColor(edge, model, _highlightColor, 5f);
		}
	}

	private string Convertmm(double value, bool appendUnit, int decimalPlace = 2)
	{
		string text = "mm";
		if (SystemConfiguration.UseInch)
		{
			value = WiCAM.Pn4000.PN3D.Converter.Convert.MmToInch(value);
			text = "inch";
			decimalPlace += 2;
		}
		if (decimalPlace < 0)
		{
			decimalPlace = 0;
		}
		string text2 = string.Empty.PadLeft(decimalPlace, '#');
		string text3 = string.Format("{0:0." + text2 + "}", value).Replace(',', '.');
		if (appendUnit)
		{
			text3 = text3 + " " + text;
		}
		return text3;
	}

	private string TranslateMacro(string macroName)
	{
		return _translator.Translate("l_popup.PnInterfaceSettings.Macro" + macroName) ?? macroName;
	}

	private string Translate(string key)
	{
		return _translator.Translate(key) ?? key;
	}

	public string ValidationResultTranslator(string key)
	{
		return ((string)Application.Current.FindResource("UnfoldView." + key)) ?? "Error";
	}

	private void MouseSelectTriangleInfo(object sender, Triangle triangle, Model model)
	{
		_highlightedEdges.Clear();
		if (_currentDoc?.EntryModel3D == null)
		{
			return;
		}
		Face face = triangle?.Face;
		PartInfo partInfo = model?.PartInfo;
		if (face?.Shell is AuxiliaryShell auxiliaryShell && (auxiliaryShell.Type.HasFlag(AuxiliaryShellType.PurchasedPartWithCollision) || auxiliaryShell.Type.HasFlag(AuxiliaryShellType.PurchasedPartIgnoreCollision)))
		{
			List<string> list = new List<string>();
			list.Add(Translate("l_popup.PopupDisassembly.PurchasedPart"));
			if (auxiliaryShell.Type.HasFlag(AuxiliaryShellType.PurchasedPartIgnoreCollision))
			{
				list.Add(Translate("l_popup.PopupUnfoldInfo.PPMountAfterBend"));
			}
			else
			{
				list.Add(Translate("l_popup.PopupUnfoldInfo.PPMountBeforeBend"));
			}
			Screen3D.SetScreenInfoText(list);
			HighlightFaces(auxiliaryShell.Faces, model);
			return;
		}
		if (partInfo != null && partInfo.PurchasedPart > 0)
		{
			List<string> list2 = new List<string>();
			list2.Add(Translate("l_popup.PopupDisassembly.PurchasedPart"));
			if (partInfo.IgnoreCollision)
			{
				list2.Add(Translate("l_popup.PopupUnfoldInfo.PPMountAfterBend"));
			}
			else
			{
				list2.Add(Translate("l_popup.PopupUnfoldInfo.PPMountBeforeBend"));
			}
			Screen3D.SetScreenInfoText(list2);
			HighlightFaces(model.GetRootParent().GetAllFaces(), model);
			return;
		}
		if (face != null && face.FaceGroup?.IsBendingZone == true)
		{
			FaceGroup bending = face.FaceGroup.ParentGroup ?? face.FaceGroup;
			List<string> list3 = new List<string>();
			list3.Add(Translate("l_popup.PopupUnfoldSetting.ValiGroupBending"));
			list3.Add(Translate("UnfoldView.ValiIntrinsicInnerR") + " = " + Convertmm(bending.ConcaveAxis.Radius, appendUnit: true));
			list3.Add(Translate("UnfoldView.ValiIntrinsicOuterR") + " = " + Convertmm(0.0 - bending.ConvexAxis.Radius, appendUnit: true));
			list3.Add(Translate("UnfoldView.AngleDeg") + " = " + $"{bending.ConvexAxis.OpeningAngle * 180.0 / Math.PI:0.###}".Replace(',', '.') + "°");
			ICombinedBendDescriptorInternal combinedBendDescriptorInternal = _currentDoc?.CombinedBendDescriptors?.FirstOrDefault((ICombinedBendDescriptorInternal cbf) => cbf != null && cbf.Enumerable?.Any((IBendDescriptor bz) => bz.BendParams.BendFaceGroup.ID == bending.ID) == true);
			if (combinedBendDescriptorInternal != null)
			{
				VisualBendInfoItems visualBendInfoItems = new VisualBendInfoItems(combinedBendDescriptorInternal, null, null, null);
				list3.Add(Translate("l_popup.PopupUnfoldInfo.OriginalRadius") + " " + visualBendInfoItems.OriginalRadius.Converted);
				list3.Add(Translate("l_popup.PopupUnfoldInfo.BendDeduction") + " " + visualBendInfoItems.BendDeduction.Converted);
				list3.Add(Translate("l_popup.PopupUnfoldInfo.KFactor") + " " + visualBendInfoItems.KFactor.ToString("0.#####"));
				list3.Add(Translate("l_popup.PopupUnfoldInfo.Algorithm") + ": " + visualBendInfoItems.KFactorAlgorithmTranslated);
				list3.Add(Translate("l_popup.PopupUnfoldInfo.Number") + " " + visualBendInfoItems.UiOrder);
			}
			List<ValidationResult> validationResults = _currentDoc.ValidationResults;
			if (validationResults != null)
			{
				ValidationResult res2 = validationResults.FirstOrDefault((ValidationResult x) => x.Type == ValidationResult.ResultTypes.BendingGroup && x.BendingGroup.ID == bending.ID);
				printValidationResults(res2, list3);
			}
			Screen3D.SetScreenInfoText(list3);
			HighlightFaces(bending.GetAllFaces(), model);
			return;
		}
		if (face?.Macro != null)
		{
			Macro macro = face.Macro;
			List<string> list4 = new List<string>();
			if (!(macro is SimpleHole simpleHole))
			{
				if (!(macro is TwoSidedCounterSink twoSidedCounterSink))
				{
					if (!(macro is CounterSink counterSink))
					{
						if (!(macro is StepDrilling stepDrilling))
						{
							if (!(macro is EmbossedCounterSink embossedCounterSink))
							{
								if (!(macro is EmbossedFreeform embossedFreeform))
								{
									if (!(macro is Louver louver))
									{
										if (!(macro is Lance))
										{
											if (!(macro is BridgeLance bridgeLance))
											{
												if (!(macro is Chamfer chamfer))
												{
													if (!(macro is Border border))
													{
														if (!(macro is BlindHole))
														{
															if (!(macro is ConicBlindHole conicBlindHole))
															{
																if (!(macro is SphericalBlindHole sphericalBlindHole))
																{
																	if (!(macro is EmbossedCircle))
																	{
																		if (!(macro is EmbossedSquare))
																		{
																			if (!(macro is EmbossedSquareRounded))
																			{
																				if (!(macro is EmbossedRectangle))
																				{
																					if (!(macro is EmbossedRectangleRounded))
																					{
																						if (!(macro is Bolt))
																						{
																							if (!(macro is EmbossmentStamp embossmentStamp))
																							{
																								if (!(macro is Deepening deepening))
																								{
																									if (!(macro is EmbossedLine))
																									{
																										if (!(macro is PressNut))
																										{
																											if (!(macro is Thread))
																											{
																												if (macro is ManufacturingMacro { ManufacturingData: var manufacturingData })
																												{
																													if (!(manufacturingData is SManufacturingDataHoleBase sManufacturingDataHoleBase))
																													{
																														if (manufacturingData is SManufacturingDataThreadBase)
																														{
																															list4.Add(TranslateMacro("SManufacturingDataThreadBase"));
																														}
																														else
																														{
																															list4.Add(TranslateMacro("ManufacturingMacroFallback"));
																														}
																													}
																													else
																													{
																														list4.Add(TranslateMacro("SManufacturingDataHoleBase") + " " + sManufacturingDataHoleBase.Thread?.Designation);
																														list4.Add(sManufacturingDataHoleBase.FeatureName);
																														if (sManufacturingDataHoleBase.Thread != null)
																														{
																															list4.Add(sManufacturingDataHoleBase.Thread?.DesignationSystem.ToString());
																															list4.Add("⌀ = " + sManufacturingDataHoleBase.Thread?.Diameter);
																															list4.Add(_translator.Translate("UnfoldView.ValiIntrinsicDepth") + " " + sManufacturingDataHoleBase.Thread?.Depth);
																														}
																													}
																												}
																												else
																												{
																													list4.Add(TranslateMacro(""));
																												}
																											}
																											else
																											{
																												list4.Add(TranslateMacro("Thread"));
																											}
																										}
																										else
																										{
																											list4.Add(TranslateMacro("PressNut"));
																										}
																									}
																									else
																									{
																										list4.Add(TranslateMacro("EmbossedLine"));
																									}
																								}
																								else if (deepening.IsSpecialAny)
																								{
																									if (deepening.IsSpecialVisible)
																									{
																										list4.Add(Translate("UnfoldView.InfoEmbStampVisibleFace"));
																									}
																									if (deepening.IsSpecialDirectionGrinding)
																									{
																										list4.Add(Translate("UnfoldView.InfoEmbStampGrinding"));
																									}
																								}
																								else
																								{
																									list4.Add(TranslateMacro("Deepening"));
																									list4.Add(Translate("UnfoldView.ValiIntrinsicDepth") + " " + Convertmm(deepening.Depth, appendUnit: true));
																								}
																							}
																							else if (embossmentStamp.IsSpecialAny)
																							{
																								if (embossmentStamp.IsSpecialVisible)
																								{
																									list4.Add(Translate("UnfoldView.InfoEmbStampVisibleFace"));
																								}
																								if (embossmentStamp.IsSpecialDirectionGrinding)
																								{
																									list4.Add(Translate("UnfoldView.InfoEmbStampGrinding"));
																								}
																							}
																							else
																							{
																								list4.Add(TranslateMacro("EmbossmentStamp"));
																							}
																						}
																						else
																						{
																							list4.Add(TranslateMacro("Bolt"));
																						}
																					}
																					else
																					{
																						list4.Add(TranslateMacro("EmbossedRectangleRounded"));
																					}
																				}
																				else
																				{
																					list4.Add(TranslateMacro("EmbossedRectangle"));
																				}
																			}
																			else
																			{
																				list4.Add(TranslateMacro("EmbossedSquareRounded"));
																			}
																		}
																		else
																		{
																			list4.Add(TranslateMacro("EmbossedSquare"));
																		}
																	}
																	else
																	{
																		list4.Add(TranslateMacro("EmbossedCircle"));
																	}
																}
																else
																{
																	list4.Add(TranslateMacro("SphericalBlindHole"));
																	list4.Add(Translate("UnfoldView.ValiIntrinsicOuterR") + " " + Convertmm(sphericalBlindHole.CylinderRadius, appendUnit: true));
																	list4.Add(Translate("UnfoldView.ValiIntrinsicDepth") + " " + Convertmm(sphericalBlindHole.CylinderDepth, appendUnit: true));
																}
															}
															else
															{
																list4.Add(TranslateMacro("ConicBlindHole"));
																list4.Add(Translate("UnfoldView.ValiIntrinsicOuterR") + " " + Convertmm(conicBlindHole.CylinderRadius, appendUnit: true));
																list4.Add(Translate("UnfoldView.ValiIntrinsicInnerR") + " " + Convertmm(conicBlindHole.TruncationRadius, appendUnit: true));
																list4.Add(Translate("UnfoldView.ValiIntrinsicDepth") + " " + Convertmm(conicBlindHole.CylinderDepth, appendUnit: true));
															}
														}
														else
														{
															list4.Add(TranslateMacro("BlindHole"));
														}
													}
													else
													{
														list4.Add(border.IsHole ? Translate("l_popup.PnInterfaceSettings.MacroBorderHole") : TranslateMacro("Border"));
														if (border.Length > 1E-06)
														{
															list4.Add((border.IsHole ? Translate("UnfoldView.Circumference") : Translate("l_popup.AdapterView.Length")) + $": {Convertmm(border.Length, appendUnit: true):0.0}");
														}
													}
												}
												else if (chamfer.LowerChamferAngle > 1E-06 && chamfer.UpperChamferAngle > 1E-06)
												{
													list4.Add(TranslateMacro(chamfer.HasSteg ? "KChamfer" : "XChamfer"));
													list4.Add(Translate("UnfoldView.UpperChamferAngleDeg") + $": {180.0 / Math.PI * chamfer.UpperChamferAngle:0}°");
													list4.Add(Translate("UnfoldView.UpperChamferHeight") + $": {Convertmm(chamfer.UpperChamferHeight, appendUnit: true):0.0}");
													list4.Add(Translate("UnfoldView.LowerChamferAngleDeg") + $": {180.0 / Math.PI * chamfer.LowerChamferAngle:0}°");
													list4.Add(Translate("UnfoldView.LowerChamferHeight") + $": {Convertmm(chamfer.LowerChamferHeight, appendUnit: true):0.0}");
												}
												else
												{
													bool flag = chamfer.UpperChamferAngle > 1E-06 && chamfer.LowerChamferAngle < 1E-06;
													int visibleFaceGroupSide = _currentDoc.VisibleFaceGroupSide;
													if ((uint)visibleFaceGroupSide <= 1u)
													{
														bool? isSide0Top = model.GetRootParent().GetFirstFaceOfGroup(_currentDoc.VisibleFaceGroupId, _currentDoc.VisibleFaceGroupSide).FaceGroup.IsSide0Top;
														bool? flag2 = face.FaceGroup?.IsSide0Top;
														string text;
														if (!flag2.HasValue || !isSide0Top.HasValue)
														{
															text = "Chamfer";
														}
														else
														{
															bool flag3 = (_currentDoc.VisibleFaceGroupSide == 0) ^ !flag;
															text = ((!isSide0Top.Equals(flag2)) ? (flag3 ? "LowerChamfer" : "UpperChamfer") : (flag3 ? "UpperChamfer" : "LowerChamfer"));
														}
														if (chamfer.HasSteg)
														{
															text += "WithStep";
														}
														list4.Add(TranslateMacro(text));
													}
													else
													{
														list4.Add(TranslateMacro("Chamfer"));
													}
													list4.Add(Translate("UnfoldView.AngleDeg") + $": {180.0 / Math.PI * (flag ? chamfer.UpperChamferAngle : chamfer.LowerChamferAngle):0}°");
													list4.Add(Translate("UnfoldView.Height") + $": {Convertmm(flag ? chamfer.UpperChamferHeight : chamfer.LowerChamferHeight, appendUnit: true):0.0}");
												}
											}
											else
											{
												list4.Add(TranslateMacro("BridgeLance") + " " + bridgeLance.ID);
											}
										}
										else
										{
											list4.Add(TranslateMacro("Lance"));
										}
									}
									else
									{
										list4.Add(TranslateMacro("Louver") + " " + louver.ID + ":");
										list4.Add(Translate("UnfoldView.ValiIntrinsicDepth") + " " + Convertmm(louver.Size0, appendUnit: true) + " - " + Convertmm(louver.Size1, appendUnit: true) + " - " + Convertmm(louver.Size2, appendUnit: true));
									}
								}
								else
								{
									list4.Add(TranslateMacro("EmbossedFreeform"));
									list4.Add(Translate("UnfoldView.ValiIntrinsicDepth") + " " + Convertmm(embossedFreeform.Depth, appendUnit: true));
								}
							}
							else
							{
								list4.Add(TranslateMacro("EmbossedCounterSink"));
								list4.Add(Translate("UnfoldView.ValiIntrinsicRadius") + " " + Convertmm(embossedCounterSink.OuterRadius, appendUnit: true) + " - " + Convertmm(embossedCounterSink.MiddleRadius, appendUnit: true) + " - " + Convertmm(embossedCounterSink.InnerRadius, appendUnit: true));
							}
						}
						else
						{
							list4.Add(TranslateMacro("StepDrilling"));
							string text2 = Translate("UnfoldView.ValiIntrinsicRadius") + " " + Convertmm(stepDrilling.Side0Radius, appendUnit: true) + " - ";
							if (stepDrilling.Steps == 3)
							{
								text2 = text2 + Convertmm(stepDrilling.InnerRadius, appendUnit: true) + " - ";
							}
							text2 += Convertmm(stepDrilling.Side1Radius, appendUnit: true);
							list4.Add(text2);
						}
					}
					else
					{
						list4.Add(TranslateMacro("CounterSink"));
						list4.Add(Translate("UnfoldView.ValiIntrinsicRadius") + " " + Convertmm(counterSink.Side0Radius, appendUnit: true) + " - " + Convertmm(counterSink.Side1Radius, appendUnit: true));
						list4.Add(Translate("UnfoldView.AngleDeg") + " " + Math.Round(counterSink.Angle, 2) + "°");
					}
				}
				else
				{
					list4.Add(TranslateMacro("TwoSidedCounterSink"));
					list4.Add(Translate("UnfoldView.ValiIntrinsicOuterR") + " " + Convertmm(twoSidedCounterSink.Side0Radius, appendUnit: true));
					list4.Add(Translate("UnfoldView.ValiIntrinsicDepth") + " " + Convertmm(twoSidedCounterSink.Side0Depth, appendUnit: true));
					list4.Add(Translate("UnfoldView.AngleDeg") + " " + Math.Round(twoSidedCounterSink.Side0Angle, 2) + "°");
					list4.Add(Translate("UnfoldView.ValiIntrinsicOuterR") + " " + Convertmm(twoSidedCounterSink.Side1Radius, appendUnit: true));
					list4.Add(Translate("UnfoldView.ValiIntrinsicDepth") + " " + Convertmm(twoSidedCounterSink.Side1Depth, appendUnit: true));
					list4.Add(Translate("UnfoldView.AngleDeg") + " " + Math.Round(twoSidedCounterSink.Side1Angle, 2) + "°");
					list4.Add(Translate("UnfoldView.ValiIntrinsicInnerR") + " " + Convertmm(twoSidedCounterSink.InnerRadius, appendUnit: true));
				}
			}
			else
			{
				list4.Add(TranslateMacro("SimpleHole"));
				list4.Add("⌀ = " + Convertmm(simpleHole.Radius * 2.0, appendUnit: true));
			}
			if (list4.Count > 0)
			{
				List<ValidationResult> validationResults2 = _currentDoc.ValidationResults;
				if (validationResults2 != null)
				{
					ValidationResult res3 = validationResults2.FirstOrDefault((ValidationResult x) => x.Type == ValidationResult.ResultTypes.Macro && x.Macro.ID == macro.ID);
					printValidationResults(res3, list4);
				}
				Screen3D.SetScreenInfoText(list4);
				HighlightFaces(macro.Faces, model);
				return;
			}
		}
		Screen3D.HideScreenInfoText();
		void printValidationResults(ValidationResult res, List<string> infos)
		{
			if (_currentViewMode != 0)
			{
				if (res != null)
				{
					infos.Add("");
					if (res.IntrinsicErrors != null)
					{
						foreach (ValidationResultIntrinsic intrinsicError in res.IntrinsicErrors)
						{
							infos.Add(Translate("UnfoldView.ValiIntrinsicWarningLog") + " " + intrinsicError.Translate(ValidationResultTranslator));
						}
					}
					if (res.DistanceErrors != null)
					{
						foreach (ValidationResult distanceError in res.DistanceErrors)
						{
							infos.Add(Translate("UnfoldView.ValiDistanceWarningLog") + " " + distanceError.Desc);
						}
					}
				}
				else
				{
					infos.Add("");
					infos.Add(Translate("l_popup.PopupUnfoldInfo.ValidationOK"));
				}
			}
		}
	}

	private void HighlightFaces(IEnumerable<Face> faces, Model model)
	{
		foreach (Face face in faces)
		{
			foreach (FaceHalfEdge allEdge in face.GetAllEdges())
			{
				_highlightedEdges.Add((allEdge, model));
			}
		}
	}
}
