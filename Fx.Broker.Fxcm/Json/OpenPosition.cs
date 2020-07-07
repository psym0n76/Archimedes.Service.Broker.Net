/*
 * Copyright (c) 2018: FXCM Group, LLC 
 *
 * FXCM Group, LLC and each of its affiliates and subsidiaries are herein referred 
 * to as, "FXCM".
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation 
 *    and/or other materials provided with the distribution.
 * 3. FXCM's name may not be used to endorse or promote products derived from
 *    this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY FXCM "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND 
 * FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL FXCM BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
 * GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
 * THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
using Newtonsoft.Json;
using System;

namespace Fx.Broker.Fxcm.Json
{
    /// <summary>
    /// OpenPosition model Json entity.
    /// </summary>
    public class OpenPosition
    {
        /// <summary>
        /// The update action. Optional. It's defined only in updates.
        /// </summary>
        [JsonProperty("action")]
        [JsonConverter(typeof(Converters.UpdateActionConverter))]
        public Fx.Broker.Fxcm.UpdateAction? Action { get; set; }

        /// <summary>
        /// The price precision of the instrument. It defines number of digits after the decimal point in the instrument price quote.
        /// </summary>
        [JsonProperty("ratePrecision")]
        public int? RatePrecision { get; set; }

        /// <summary>
        /// The unique identification number of the open position. The number is unique within the same database that stores the account the position is opened on.
        /// </summary>
        [JsonProperty("tradeId")]
        public string TradeId { get; set; }

        /// <summary>
        /// The unique name of the account the position is opened on. The name is unique within the database where the account is stored.
        /// </summary>
        [JsonProperty("accountName")]
        public string AccountName { get; set; }

        /// <summary>
        /// The unique identification number of the account the position is opened on. The number is unique within the database where the account is stored.
        /// </summary>
        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        /// <summary>
        /// The cumulative amount of funds that is added the account balance for holding the position overnight.
        /// </summary>
        [JsonProperty("roll")]
        public double? Roll { get; set; }

        /// <summary>
        /// The amount of funds subtracted from the account balance to pay for the broker's service in accordance with the terms and conditions of the account trading agreement.
        /// </summary>
        [JsonProperty("com")]
        public double? Com { get; set; }

        /// <summary>
        /// The price the position is opened at.
        /// </summary>
        [JsonProperty("open")]
        public double? Open { get; set; }

        /// <summary>
        /// The simulated delivery date. The date when the position could be automatically closed. The date is provided in the yyyyMMdd format. It is applicable only for positions opened on accounts with the day netting trading mode. Otherwise, the default DateTime.
        /// </summary>
        [JsonProperty("valueDate")]
        [JsonConverter(typeof(Converters.ValueDateConverter))]
        public DateTime? ValueDate { get; set; }

        /// <summary>
        /// The current profit/loss of the position. It is expressed in the account currency.
        /// </summary>
        [JsonProperty("grossPL")]
        public double? GrossPL { get; set; }

        /// <summary>
        /// The price at which the position can be closed at the moment.
        /// </summary>
        [JsonProperty("close")]
        public double? Close { get; set; }

        /// <summary>
        /// The current profit/loss per one lot of the position. It is expressed in the account currency.
        /// </summary>
        [JsonProperty("visiblePL")]
        public double? VisiblePL { get; set; }

        /// <summary>
        /// unknown
        /// </summary>
        [JsonProperty("isDisabled")]
        public bool? IsDisabled { get; set; }

        /// <summary>
        /// The symbol of the instrument.
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// The trade operation the position is opened by.
        /// </summary>
        [JsonProperty("isBuy")]
        public bool? IsBuy { get; set; }

        /// <summary>
        /// The amount of the position in thousand units.
        /// </summary>
        [JsonProperty("amountK")]
        public int? AmountK { get; set; }

        /// <summary>
        /// unknown
        /// </summary>
        [JsonProperty("currencyPoint")]
        public double? CurrencyPoint { get; set; }

        /// <summary>
        /// The date and time when the position was opened.
        /// </summary>
        [JsonProperty("time")]
        [JsonConverter(typeof(Converters.TimeConverter))]
        public DateTime? Time { get; set; }

        /// <summary>
        /// The amount of funds currently committed to maintain the position.
        /// </summary>
        [JsonProperty("usedMargin")]
        public double? UsedMargin { get; set; }

        /// <summary>
        /// The price of the associated stop order (loss limit level).
        /// </summary>
        [JsonProperty("stop")]
        public double? Stop { get; set; }

        /// <summary>
        /// The number of pips the market should move before the stop order moves the same number of pips after it. If the trailing order is dynamic (automatically updates every 0.1 of a pip), then the value of this field is 1. If the order is not trailing, the value of this field is 0.
        /// </summary>
        [JsonProperty("stopMove")]
        public double? StopMove { get; set; }

        /// <summary>
        /// The price of the associated limit order (profit limit level).
        /// </summary>
        [JsonProperty("limit")]
        public double? Limit { get; set; }

        /// <summary>
        /// Indicates the row is a summary of for whole table.
        /// </summary>
        [JsonProperty("isTotal")]
        public bool? IsTotal { get; set; }

        /// <summary>
        /// Converts Json object to a model entity.
        /// </summary>
        /// <returns>The entity to return to a user.</returns>
        /// <exception cref="Exception">If a response is an error response.</exception>
        public Fx.Broker.Fxcm.Models.OpenPosition ToEntity()
        {
            var entity = new Fx.Broker.Fxcm.Models.OpenPosition();
            if (RatePrecision != null)
            {
                entity.RatePrecision = RatePrecision.Value;
                entity.IsRatePrecisionValid = true;
            }
            if (TradeId != null)
            {
                entity.TradeId = TradeId;
                entity.IsTradeIdValid = true;
            }
            if (AccountName != null)
            {
                entity.AccountName = AccountName;
                entity.IsAccountNameValid = true;
            }
            if (AccountId != null)
            {
                entity.AccountId = AccountId;
                entity.IsAccountIdValid = true;
            }
            if (Roll != null)
            {
                entity.Roll = Roll.Value;
                entity.IsRollValid = true;
            }
            if (Com != null)
            {
                entity.Com = Com.Value;
                entity.IsComValid = true;
            }
            if (Open != null)
            {
                entity.Open = Open.Value;
                entity.IsOpenValid = true;
            }
            if (ValueDate != null)
            {
                entity.ValueDate = ValueDate.Value;
                entity.IsValueDateValid = true;
            }
            if (GrossPL != null)
            {
                entity.GrossPL = GrossPL.Value;
                entity.IsGrossPLValid = true;
            }
            if (Close != null)
            {
                entity.Close = Close.Value;
                entity.IsCloseValid = true;
            }
            if (VisiblePL != null)
            {
                entity.VisiblePL = VisiblePL.Value;
                entity.IsVisiblePLValid = true;
            }
            if (IsDisabled != null)
            {
                entity.IsDisabled = IsDisabled.Value;
                entity.IsIsDisabledValid = true;
            }
            if (Currency != null)
            {
                entity.Currency = Currency;
                entity.IsCurrencyValid = true;
            }
            if (IsBuy != null)
            {
                entity.IsBuy = IsBuy.Value;
                entity.IsIsBuyValid = true;
            }
            if (AmountK != null)
            {
                entity.AmountK = AmountK.Value;
                entity.IsAmountKValid = true;
            }
            if (CurrencyPoint != null)
            {
                entity.CurrencyPoint = CurrencyPoint.Value;
                entity.IsCurrencyPointValid = true;
            }
            if (Time != null)
            {
                entity.Time = Time.Value;
                entity.IsTimeValid = true;
            }
            if (UsedMargin != null)
            {
                entity.UsedMargin = UsedMargin.Value;
                entity.IsUsedMarginValid = true;
            }
            if (Stop != null)
            {
                entity.Stop = Stop.Value;
                entity.IsStopValid = true;
            }
            if (StopMove != null)
            {
                entity.StopMove = StopMove.Value;
                entity.IsStopMoveValid = true;
            }
            if (Limit != null)
            {
                entity.Limit = Limit.Value;
                entity.IsLimitValid = true;
            }
            if (IsTotal != null)
            {
                entity.IsTotal = IsTotal.Value;
                entity.IsIsTotalValid = true;
            }
            return entity;
        }
    }
}
